package utils

import (
	"fmt"
	"os"
	"path/filepath"
)

// ProjectRoot returns the absolute path to the go_report_generator directory
// by searching for the go.mod file in parent directories
func ProjectRoot() string {
	// Start with the working directory
	dir, err := os.Getwd()
	if err != nil {
		panic(fmt.Sprintf("failed to get working directory: %v", err))
	}

	// Keep going up until we find go.mod
	for {
		// Check if go.mod exists in the current directory
		if _, err := os.Stat(filepath.Join(dir, "go.mod")); err == nil {
			return dir
		}

		// Get the parent directory
		parent := filepath.Dir(dir)
		if parent == dir {
			// We've reached the root directory without finding go.mod
			panic("could not find project root (no go.mod file found in parent directories)")
		}
		dir = parent
	}
}
