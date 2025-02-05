import path = require('path');
import tl = require('azure-pipelines-task-lib/task');

async function executeReportGenerator(): Promise<number> {
    var tool = tl.tool('dotnet')
    .arg(path.join(__dirname, 'tools/net8.0/ReportGenerator.dll'))
    .arg('-reports:' + (tl.getInput('reports') || ''))
    .arg('-targetdir:' + (tl.getInput('targetdir') || ''))
    .arg('-reporttypes:' + (tl.getInput('reporttypes') || ''))
    .arg('-sourcedirs:' + (tl.getInput('sourcedirs') || ''))
    .arg('-historydir:' + (tl.getInput('historydir') || ''))
    .arg('-plugins:' + (tl.getInput('plugins') || ''))
    .arg('-assemblyfilters:' + (tl.getInput('assemblyfilters') || ''))
    .arg('-classfilters:' + (tl.getInput('classfilters') || ''))
    .arg('-filefilters:' + (tl.getInput('filefilters') || ''))
    .arg('-riskhotspotassemblyfilters:' + (tl.getInput('riskhotspotassemblyfilters') || ''))
    .arg('-riskhotspotclassfilters:' + (tl.getInput('riskhotspotclassfilters') || ''))
    .arg('-verbosity:' + (tl.getInput('verbosity') || ''))
    .arg('-title:' + (tl.getInput('title') || ''))
    .arg('-tag:' + (tl.getInput('tag') || ''))
    .arg('-license:' + (tl.getInput('license') || ''));

    const customSettings = (tl.getInput('customSettings') || '');

    if (customSettings.length > 0) {
        customSettings.split(/[,;]/).forEach(setting => {
            tool = tool.arg(setting.trim());
        });
    }

    return await tool.execAsync();
}

async function run() {
    const publishCodeCoverageResults = ((tl.getInput('publishCodeCoverageResults') || 'false') + '').toLowerCase() === 'true';
    try {
        tl.setResourcePath(path.join( __dirname, 'task.json'));

        let code = await executeReportGenerator();
        if (code != 0) {
            tl.setResult(tl.TaskResult.Failed, tl.loc('FailedMsg'));
            return;
        }

        if (!publishCodeCoverageResults) {
            tl.setResult(tl.TaskResult.Succeeded, tl.loc('SucceedMsg'));
            return;
        }
    }
    catch (e) {
        tl.debug(e.message);
        tl.setResult(tl.TaskResult.Failed, e.message);
    }
    
    try {
        publishCodeCoverageReport();
    }
    catch (e) {
        tl.debug(e.message);
        tl.setResult(tl.TaskResult.Failed, tl.loc('FailedToPublishReportMsg') + ': ' + e.message);
        return;
    }
}

function publishCodeCoverageReport() {
    if (((tl.getInput('publishCodeCoverageResults') || 'false') + '').toLowerCase() !== 'true') {
        return;
    }

    const targetdir = resolvePathToSingleItem(tl.getInput('targetdir') || '');
    const reporttypes = (tl.getInput('reporttypes') || '').toLowerCase().split(/[,;]/);
    const createSubdirectoryForAllReportTypes = (tl.getInput('customSettings') || '').toLowerCase().indexOf('createsubdirectoryforallreporttypes=true') > -1;

    if (!reporttypes.find(r => r === 'cobertura')) {
        tl.setResult(tl.TaskResult.Failed, tl.loc('PublishCodeCoverageResultsRequiresCobertura'));
        return;
    }

    const supportedReportTypes = ['HtmlInline_AzurePipelines', 'HtmlInline_AzurePipelines_Light', 'HtmlInline_AzurePipelines_Dark',
        'Html', 'Html_Light', 'Html_Dark', 'Html_BlueRed', 'HtmlInline', 'HtmlSummary', 'Html_BlueRed_Summary',
        'HtmlChart'];
    let htmlReportType = '';

    for (let i = 0; i < supportedReportTypes.length; i++) {
        if (reporttypes.find(r => r === supportedReportTypes[i].toLowerCase())) {
            htmlReportType = supportedReportTypes[i];
            break;
        }
    }

    if (htmlReportType === '') {
        tl.setResult(tl.TaskResult.Failed, tl.loc('PublishCodeCoverageResultsRequiresHtmlFormat'));
        return;
    }
    
    //See: https://github.com/microsoft/azure-pipelines-tasks/blob/master/Tasks/PublishCodeCoverageResultsV1/publishcodecoverageresults.ts
    const ccPublisher = new tl.CodeCoveragePublisher();
    ccPublisher.publish(
        'Cobertura', 
        targetdir + (createSubdirectoryForAllReportTypes ? '/Cobertura' : '') + '/Cobertura.xml',
        targetdir + (createSubdirectoryForAllReportTypes ? '/' + htmlReportType : ''),
        undefined);

    tl.setResult(tl.TaskResult.Succeeded, tl.loc('SucceedMsg'));
}

// Resolves the specified path to a single item based on whether it contains wildcards
function resolvePathToSingleItem(pathInput: string): string {
    // Default to using the specific pathInput value
    let resolvedPath: string = pathInput;

    if (pathInput) {
        // Find match patterns won't work if the directory has a trailing slash
        if (pathInput.endsWith('/') || pathInput.endsWith('\\')) {
            pathInput = pathInput.slice(0, -1);
        }
        // Resolve matches of the pathInput pattern
        const findOptions: tl.FindOptions = { allowBrokenSymbolicLinks: false, followSymbolicLinks: false, followSpecifiedSymbolicLink: false };
        const pathMatches: string[] = tl.findMatch(
            tl.getVariable('System.DefaultWorkingDirectory'),
            pathInput,
            findOptions);

        // Were any matches found?
        if (pathMatches.length === 0) {
            resolvedPath = undefined;
        } else {
            // Select the path to be used from the matches
            resolvedPath = pathMatches[0];

            // If more than one path matches, use the first and issue a warning
            if (pathMatches.length > 1) {
                tl.warning(tl.loc('MultipleSummaryFilesFound', resolvedPath));
            }
        }
    }

    // Return resolved path
    return resolvedPath;
}

run();