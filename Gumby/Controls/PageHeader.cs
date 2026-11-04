namespace Gumby.Controls
{
    /// <summary>
    /// Zaglavlje stranice: dugme za nazad, ikonica (emoji ili logo), naslov i podnaslov.
    /// </summary>
    public class PageHeader : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty,
                propertyChanged: (b, _, n) => ((PageHeader)b)._title.Text = (string)n);

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(PageHeader), string.Empty,
                propertyChanged: (b, _, n) => ((PageHeader)b).UpdateSubtitle());

        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(PageHeader), string.Empty,
                propertyChanged: (b, _, _) => ((PageHeader)b).UpdateIcon());

        public static readonly BindableProperty LogoSourceProperty =
            BindableProperty.Create(nameof(LogoSource), typeof(ImageSource), typeof(PageHeader), null,
                propertyChanged: (b, _, _) => ((PageHeader)b).UpdateIcon());

        public static readonly BindableProperty AccentColorProperty =
            BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(PageHeader), AppColors.Accent,
                propertyChanged: (b, _, _) => ((PageHeader)b).UpdateIcon());

        public static readonly BindableProperty ShowBackProperty =
            BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PageHeader), true,
                propertyChanged: (b, _, n) => ((PageHeader)b)._back.IsVisible = (bool)n);

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

        /// <summary>Emoji prikazan u obojenom kvadratu.</summary>
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>Logo (npr. brend) prikazan umesto emoji ikonice.</summary>
        public ImageSource? LogoSource
        {
            get => (ImageSource?)GetValue(LogoSourceProperty);
            set => SetValue(LogoSourceProperty, value);
        }

        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public bool ShowBack
        {
            get => (bool)GetValue(ShowBackProperty);
            set => SetValue(ShowBackProperty, value);
        }

        public event EventHandler? BackClicked;

        private readonly Button _back;
        private readonly Border _badge;
        private readonly Label _badgeIcon;
        private readonly Image _logo;
        private readonly Label _title;
        private readonly Label _subtitle;

        public PageHeader()
        {
            _back = new Button
            {
                Text = "←",
                HorizontalOptions = LayoutOptions.Start,
            };
            _back.SetDynamicResource(StyleProperty, "IconButton");
            _back.Clicked += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);
            SemanticProperties.SetDescription(_back, "Back");

            _badgeIcon = new Label
            {
                FontSize = 28,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            _badge = new Border
            {
                WidthRequest = 60,
                HeightRequest = 60,
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
                HorizontalOptions = LayoutOptions.Start,
                Content = _badgeIcon,
            };

            _logo = new Image
            {
                HeightRequest = 36,
                HorizontalOptions = LayoutOptions.Start,
                Aspect = Aspect.AspectFit,
                IsVisible = false,
            };

            _title = new Label { Margin = new Thickness(0, 18, 0, 0) };
            _title.SetDynamicResource(StyleProperty, "TitleText");
            SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);

            _subtitle = new Label { Margin = new Thickness(0, 6, 0, 0), IsVisible = false };
            _subtitle.SetDynamicResource(StyleProperty, "Body");

            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(0, 8, 0, 0),
                Children =
                {
                    _back,
                    new Grid
                    {
                        Margin = new Thickness(0, 24, 0, 0),
                        HorizontalOptions = LayoutOptions.Start,
                        Children = { _badge, _logo },
                    },
                    _title,
                    _subtitle,
                },
            };

            UpdateIcon();
        }

        private void UpdateSubtitle()
        {
            _subtitle.Text = Subtitle;
            _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        }

        private void UpdateIcon()
        {
            var hasLogo = LogoSource != null;
            _logo.Source = LogoSource;
            _logo.IsVisible = hasLogo;

            _badgeIcon.Text = Icon;
            _badge.IsVisible = !hasLogo && !string.IsNullOrEmpty(Icon);
            _badge.BackgroundColor = AccentColor.WithAlpha(0.14f);
            _badge.Stroke = AccentColor.WithAlpha(0.28f);
        }
    }
}
