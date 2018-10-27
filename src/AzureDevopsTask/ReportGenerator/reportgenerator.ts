import path = require('path');
import tl = require('azure-pipelines-task-lib/task');

async function executeReportGenerator(): Promise<number> {
    var tool = tl.tool('dotnet')
    .arg(path.join(__dirname, 'tools/netcoreapp2.0/ReportGenerator.dll'))
    .arg('-reports:' + (tl.getInput('reports') || ''))
    .arg('-targetdir:' + (tl.getInput('targetdir') || ''))
    .arg('-reporttypes:' + (tl.getInput('reporttypes') || ''))
    .arg('-sourcedirs:' + (tl.getInput('sourcedirs') || ''))
    .arg('-historydir:' + (tl.getInput('historydir') || ''))
    .arg('-plugins:' + (tl.getInput('plugins') || ''))
    .arg('-assemblyfilters:' + (tl.getInput('assemblyfilters') || ''))
    .arg('-classfilters:' + (tl.getInput('classfilters') || ''))
    .arg('-filefilters:' + (tl.getInput('filefilters') || ''))
    .arg('-verbosity:' + (tl.getInput('verbosity') || ''))
    .arg('-tag:' + (tl.getInput('tag') || ''));

    return await tool.exec();
}

function copyHtmIndexToHtmlIndex() {
    var sourceFile = path.join(tl.getInput('targetdir'), 'index.htm');
    if (tl.exist(sourceFile)) {
        tl.cp(sourceFile, path.join(tl.getInput('targetdir'), 'index.html'), '-f');
    }
}

async function run() {
    try {
        tl.setResourcePath(path.join( __dirname, 'task.json'));

        let code = await executeReportGenerator();

        if (code != 0) {
            tl.setResult(tl.TaskResult.Failed, tl.loc('FailedMsg'));
        }

        copyHtmIndexToHtmlIndex();

        tl.setResult(tl.TaskResult.Succeeded, tl.loc('SucceedMsg'));
    } catch (e) {
        tl.debug(e.message);
        tl.setResult(tl.TaskResult.Failed, e.message);
    }
}

run();