namespace ViewModels

open System.Windows
open System.Windows.Controls
open System.Windows.Input

type public MouseBehavior() =
    inherit FrameworkElement()

    static let mousePositionProperty = DependencyProperty.RegisterAttached("MousePosition", typeof<Point>, typeof<MouseBehavior>, UIPropertyMetadata(Point()))

    static let trackMouseProperty = 
        let trackMouseChanged (d : DependencyObject) (e : DependencyPropertyChangedEventArgs) =
            let element = d :?> FrameworkElement
            let mouseMove (e:MouseEventArgs) =
                let position = (e.GetPosition element)
                element.SetValue (mousePositionProperty, position)
            let track = e.NewValue :?> bool
            if (track) 
                then element.MouseMove.Add(mouseMove)
                else ignore()

        DependencyProperty.RegisterAttached("TrackMouse", typeof<bool>, typeof<MouseBehavior>, FrameworkPropertyMetadata(false, trackMouseChanged))

    static member TrackMouseProperty with get() = trackMouseProperty
    static member MousePositionProperty with get() = mousePositionProperty

type MyFrameworkElement() =
    inherit FrameworkElement()
   
    member this.DoMouseMove() = 
        this.RaiseEvent (MouseEventArgs(Mouse.PrimaryDevice, 0, RoutedEvent = Mouse.MouseMoveEvent, Source = this))

type TestMouseBehavior()=
    inherit FrameworkElement()

    // Repeat code from MouseBehavior to have a second test class for StartupCode elements
    static let mousePositionProperty = DependencyProperty.RegisterAttached("MousePosition", typeof<Point>, typeof<TestMouseBehavior>, UIPropertyMetadata(Point()))

    static let trackMouseProperty = 
        let trackMouseChanged (d : DependencyObject) (e : DependencyPropertyChangedEventArgs) =
            let element = d :?> FrameworkElement
            let mouseMove (e:MouseEventArgs) =
                let position = (e.GetPosition element)
                element.SetValue (mousePositionProperty, position)
            let track = e.NewValue :?> bool
            if (track) 
                then element.MouseMove.Add(mouseMove)
                else ignore()

        DependencyProperty.RegisterAttached("TrackMouse", typeof<bool>, typeof<TestMouseBehavior>, FrameworkPropertyMetadata(false, trackMouseChanged))
        
    static member MousePositionProperty with get() = mousePositionProperty

    // Now the real test method
    member this.RunTest() =
        // Arrange
        let element = MyFrameworkElement(RenderSize = Size(5., 10.))
        element.SetValue (MouseBehavior.TrackMouseProperty, true)

        // Act
        element.DoMouseMove()

        // Assert
        let point = element.GetValue(MouseBehavior.MousePositionProperty)
        point |> printfn "Point: %A"