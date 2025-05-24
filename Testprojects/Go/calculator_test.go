package calculator

import "testing"

func TestAdd(t *testing.T) {
	if Add(1, 2) != 3 {
		t.Error("Expected 1 + 2 to equal 3")
	}
	if Add(-1, 1) != 0 {
		t.Error("Expected -1 + 1 to equal 0")
	}
}

func TestSubtract(t *testing.T) {
	if Subtract(3, 2) != 1 {
		t.Error("Expected 3 - 2 to equal 1")
	}
}

func TestMultiplyZero(t *testing.T) {
	if Multiply(0, 0) != 0 {
		t.Error("Expected 0 * 0 to equal 0")
	}
}