package htmlreport

import (
	"html/template"
)

const baseLayoutTemplate = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{ .Title }}</title>
    <!-- Static CSS for overall page structure and theme -->
    <link rel="stylesheet" type="text/css" href="report.css">
    <!-- Chartist CSS for charts (if used by static part or Angular) -->
    <link rel="stylesheet" type="text/css" href="chartist.min.css">
    <!-- Angular App's CSS -->
    <link rel="stylesheet" type="text/css" href="{{ .AngularCssFile }}">
</head>
<body>
    <!-- Traditional report header - this might be removed or restyled if Angular takes over full page -->
    <h1>Report for {{ .ParserName }}</h1>
    <p>Generated on: {{ .GeneratedAt }}</p>
    
    <!-- Angular root component will be bootstrapped here -->
    <div id="content">
        <app-root></app-root>
    </div>

    <!-- Static JS for charts or other elements not handled by Angular -->
    <script type="text/javascript" src="chartist.min.js"></script>
    <script type="text/javascript" src="custom.js"></script>

    <!-- Angular App's JS files -->
    <script src="{{ .AngularRuntimeJsFile }}" type="module"></script>
    <script src="{{ .AngularPolyfillsJsFile }}" type="module"></script>
    <script src="{{ .AngularMainJsFile }}" type="module"></script>
</body>
</html>`

var (
	// baseTpl is the parsed base HTML layout template.
	// It's parsed once at package initialization for efficiency and to catch errors early.
	baseTpl = template.Must(template.New("base").Parse(baseLayoutTemplate))
)

/*
// getBaseLayoutTemplate provides access to the parsed base layout template.
// This is an alternative to using the global baseTpl directly.
func getBaseLayoutTemplate() (*template.Template, error) {
	// If baseTpl was not initialized with template.Must, it could be parsed here.
	// return template.New("base").Parse(baseLayoutTemplate)
	
	// Since baseTpl is initialized with template.Must, we can return a clone
	// if modification per-use is a concern, or the original if it's read-only.
	// For simplicity, returning the shared instance is fine if it's not modified.
	return baseTpl, nil // template.Must ensures no error here unless Parse fails at init
}
*/
