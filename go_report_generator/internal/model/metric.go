package model

// MetricStatus represents the status of a metric.
type MetricStatus int

const (
	// StatusOk indicates a normal/good status.
	StatusOk MetricStatus = iota
	// StatusWarning indicates a warning status.
	StatusWarning
	// StatusError indicates an error/critical status.
	StatusError
)

// Metric represents a single metric with a name, value, and status.
type Metric struct {
	Name   string
	Value  interface{} // To allow for different types of metric values (int, float, string)
	Status MetricStatus
}

// MethodMetric represents metrics associated with a specific method or code line.
type MethodMetric struct {
	Name    string   // Typically the method's name or a specific metric name for that method
	Line    int      // The line number where the method is defined or this metric applies
	Metrics []Metric // A slice of Metric structs associated with this method/entry
}
