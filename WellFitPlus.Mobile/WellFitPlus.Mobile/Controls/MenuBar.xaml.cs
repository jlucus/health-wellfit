using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace WellFitPlus.Mobile.Controls 
{
	public partial class MenuBar : Grid
    {
        #region Properties
        
        private string _title;
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                if (this.TitleLabel == null) { return; }
                this.TitleLabel.Text = this._title;
            }
        }

        private ExtButton _leftButton;
        public ExtButton LeftButton
        {
            get
            {
                return this._leftButton;
            }
        }

        private ExtButton _rightButton;
        public ExtButton RightButton
        {
            get
            {
                return this._rightButton;
            }
        }

        private ExtLabel _titleLabel;
        public ExtLabel TitleLabel
        {
            get
            {
                return this._titleLabel;
            }
        }

        private Image _secondImage;
        public Image SecondImage
        {
            get
            {
                return this._secondImage;
            }
        }

        public enum MenuBarType : int
        {
            MenuAndBackButton = 0,
            MenuAndSaveButton = 1,
            MenuButtonOnly = 2
        }

        private MenuBarType _type = MenuBarType.MenuAndBackButton;
        public MenuBarType Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;

                // Build Right Button
                this.BuildRightButton();
            }
        }

        #endregion

        #region Initialization

        public MenuBar ()
		{
			InitializeComponent ();

            this.LoadMenuBar();

        }

        private void LoadMenuBar()
        {
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.MENU_BAR_BACKGROUND,
                Aspect = Aspect.AspectFill
            };

            this.menuLayout.Children.Add(streakImage,
              Constraint.Constant(0),
              Constraint.Constant(0),
              Constraint.RelativeToParent((parent) => { return parent.Width; }),
              Constraint.RelativeToParent((parent) => { return parent.Height; }));

            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = 50 },
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                    new ColumnDefinition() { Width = 50}
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  50 },
                },
                RowSpacing = 0,
                ColumnSpacing = 0,
                Padding = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 },
                Margin= new Thickness(0,2,0,0),
                HorizontalOptions= LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand
            };

            Image menuImage = new Image()
            {
                Source = AppGlobals.Images.MENU_BUTTON,
                Aspect = Aspect.Fill,
                Margin = new Thickness() { Left = 10, Right = 10, Top = 11, Bottom = 11 }
            };

            Grid.SetColumn(menuImage, 0);
            Grid.SetRow(menuImage, 0);
            grid.Children.Add(menuImage);

            this._leftButton = new ExtButton()
            {
                Text = "",
                Margin = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 },
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            
            Grid.SetColumn(_leftButton, 0);
            Grid.SetRow(_leftButton, 0);
            grid.Children.Add(_leftButton);

            this._titleLabel = new ExtLabel()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontFamily = "Raleway-Regular",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                Text = this.Title,
                TextColor = Color.White,
                Margin= new Thickness(0,2,0,0)
            };

            Grid.SetColumn(_titleLabel, 1);
            Grid.SetRow(_titleLabel, 0);
            grid.Children.Add(_titleLabel);

            this._secondImage = new Image()
            {
                Source = AppGlobals.Images.BACK_BUTTON,
                Aspect = Aspect.Fill,
                Margin = new Thickness() { Left = 10, Right = 10, Top = 10, Bottom = 10 },
                IsVisible = false
            };

            Grid.SetColumn(_secondImage, 2);
            Grid.SetRow(_secondImage, 0);
            grid.Children.Add(_secondImage);

            this._rightButton = new ExtButton()
            {
                Text = "",
                Margin = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 },
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };

            Grid.SetColumn(_rightButton, 2);
            Grid.SetRow(_rightButton, 0);
            grid.Children.Add(_rightButton);

            this.menuLayout.Children.Add(grid,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }

        #endregion

        #region Functions

        private void BuildRightButton()
        {
            switch (this._type)
            {
                case MenuBarType.MenuAndBackButton:
                    this.SecondImage.Source = AppGlobals.Images.BACK_BUTTON;
                    break;
                case MenuBarType.MenuAndSaveButton:
                    this.SecondImage.Source = "";
                    this._rightButton.BorderWidth = 2;
                    this._rightButton.FontFamily = "Raleway-Regular";
                    this._rightButton.BorderRadius = 17;
                    this._rightButton.BorderColor = Color.White;
                    this._rightButton.Margin = new Thickness() { Left = -32, Top = 6, Right = 4, Bottom = 5};
                    this._rightButton.VerticalOptions = LayoutOptions.Center;
                    this._rightButton.TextColor = Color.White;
                    this._rightButton.Text = "SAVE";
                    break;
                case MenuBarType.MenuButtonOnly:
                    this._rightButton.IsVisible = false;
                    this._secondImage.IsVisible = false;
                    break;
            }
        }

        #endregion
    }
}
