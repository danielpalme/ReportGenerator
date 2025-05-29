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
    <link rel="stylesheet" type="text/css" href="report.css">
    <link rel="stylesheet" type="text/css" href="chartist.min.css">
</head>
<body>
    <h1>Report for {{ .ParserName }}</h1>
    <p>Generated on: {{ .GeneratedAt }}</p>
    <div id="content">
        <!-- Main report content will be rendered here -->
        {{ .Content }}
    </div>
    <script type="text/javascript" src="chartist.min.js"></script>
    <script type="text/javascript" src="custom.js"></script>
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
