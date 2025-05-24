package calculator

// Add returns the sum of two integers.
func Add(a, b int) int {
	return a + b
}

// Subtract returns the difference between two integers.
func Subtract(a, b int) int {
	return a - b
}

// Multiply returns the product of two integers.
// This function is intentionally not fully covered by tests.
func Multiply(a, b int) int {
	if a == 0 || b == 0 {
		return 0
	}
	return a * b
}