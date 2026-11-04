using Microsoft.Maui.Controls.Shapes;

namespace Gumby.Controls
{
    /// <summary>
    /// Kartica akcije: obojena ikonica, naslov i opis. Koristi se u mrežama izbora.
    /// Kada je <see cref="IsRow"/> true, prikazuje se kao red (ikonica levo, strelica desno).
    /// </summary>
    public class ActionTile : ContentView
    {
        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(ActionTile), string.Empty,
                propertyChanged: (b, _, _) => ((ActionTile)b).Update());

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ActionTile), string.Empty,
                propertyChanged: (b, _, _) => ((ActionTile)b).Update());

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(ActionTile), string.Empty,
                propertyChanged: (b, _, _) => ((ActionTile)b).Update());

        public static readonly BindableProperty AccentColorProperty =
            BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(ActionTile), AppColors.Accent,
                propertyChanged: (b, _, _) => ((ActionTile)b).Update());

        public static readonly BindableProperty IsRowProperty =
            BindableProperty.Create(nameof(IsRow), typeof(bool), typeof(ActionTile), false,
                propertyChanged: (b, _, _) => { var t = (ActionTile)b; t.BuildLayout(); t.Update(); });

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public bool IsRow
        {
            get => (bool)GetValue(IsRowProperty);
            set => SetValue(IsRowProperty, value);
        }

        public event EventHandler? Clicked;

        private readonly Border _card;
        private readonly Border _badge;
        private readonly Label _icon;
        private readonly Label _title;
        private readonly Label _subtitle;
        private readonly Label _chevron;

        public ActionTile()
        {
            _icon = new Label
            {
                FontSize = 22,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            _badge = new Border
            {
                WidthRequest = 46,
                HeightRequest = 46,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = _icon,
            };

            _title = new Label { LineBreakMode = LineBreakMode.TailTruncation };
            _title.SetDynamicResource(StyleProperty, "Heading");

            _subtitle = new Label { MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation };
            _subtitle.SetDynamicResource(StyleProperty, "Caption");

            _chevron = new Label
            {
                Text = "›",
                FontSize = 26,
                VerticalOptions = LayoutOptions.Center,
                TextColor = AppColors.TextMuted,
            };

            _card = new Border();
            _card.SetDynamicResource(StyleProperty, "Card");

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            GestureRecognizers.Add(tap);

            Content = _card;
            BuildLayout();
            Update();
        }

        private void BuildLayout()
        {
            // Elementi se premeštaju između rasporeda, pa ih prvo odvoji od starog roditelja
            foreach (var view in new View[] { _badge, _title, _subtitle, _chevron })
                (view.Parent as Layout)?.Children.Remove(view);

            if (IsRow)
            {
                _card.Padding = new Thickness(14);
                _card.MinimumHeightRequest = 0;
                _title.Margin = 0;

                var text = new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center,
                    Children = { _title, _subtitle },
                };

                var grid = new Grid
                {
                    ColumnSpacing = 14,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto),
                    },
                };
                _badge.VerticalOptions = LayoutOptions.Center;
                grid.Add(_badge, 0);
                grid.Add(text, 1);
                grid.Add(_chevron, 2);
                _card.Content = grid;
            }
            else
            {
                _card.Padding = new Thickness(16);
                _title.Margin = new Thickness(0, 16, 0, 2);
                _badge.VerticalOptions = LayoutOptions.Start;

                _card.Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children = { _badge, _title, _subtitle },
                };
            }
        }

        private void Update()
        {
            _icon.Text = Icon;
            _title.Text = Title;
            _subtitle.Text = Subtitle;
            _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);

            // Kartice sa opisom imaju istu minimalnu visinu, kompaktne (samo naslov) su niže
            _card.MinimumHeightRequest = IsRow || string.IsNullOrEmpty(Subtitle) ? 0 : 136;
            _badge.BackgroundColor = AccentColor.WithAlpha(0.15f);

            SemanticProperties.SetDescription(this, string.IsNullOrEmpty(Subtitle) ? Title : $"{Title}. {Subtitle}");
        }

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);

            // Kratka povratna animacija pritiska
            this.CancelAnimations();
            await this.ScaleToAsync(0.96, 70, Easing.CubicOut);
            await this.ScaleToAsync(1, 120, Easing.CubicOut);
        }
    }
}
