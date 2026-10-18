namespace Mobile.Extensions;

public class TouchFeedbackBehavior : Behavior<View>
{
    private View? _associatedView;
    private readonly PointerGestureRecognizer _pointerRecognizer = new();

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _associatedView = bindable;

        _pointerRecognizer.PointerEntered += OnPointerEntered;
        _pointerRecognizer.PointerExited += OnPointerExited;
        bindable.GestureRecognizers.Add(_pointerRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.GestureRecognizers.Remove(_pointerRecognizer);
        _pointerRecognizer.PointerEntered -= OnPointerEntered;
        _pointerRecognizer.PointerExited -= OnPointerExited;
        _associatedView = null;
    }

    private async void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_associatedView != null)
        {
            await _associatedView.ScaleTo(1.02, 100, Easing.CubicOut);
        }
    }

    private async void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_associatedView != null)
        {
            await _associatedView.ScaleTo(1.0, 100, Easing.CubicIn);
        }
    }
}
