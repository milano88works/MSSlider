using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace milano88.UI.Controls
{
    [DefaultEvent("ValueChanged")]

    public class MSSliderHorizontal : Control
    {
        public MSSliderHorizontal()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.Width = 100;
            this.Value = 50;
            this.BackColor = Color.Transparent;
            UpdateGraphicsBuffer();
        }

        #region Default Event
        [Description("Occurs when the scroll percentage is changed")]
        public event EventHandler ValueChanged;
        #endregion

        #region Overrides
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseOverSlider = false;
            this.Invalidate(_knobRect);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isMouseOverSlider = false;
            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool maxedLeft = e.X < 0 && Percent == 0F;
            bool maxedRight = e.X > this.Width && Percent == 1F;
            if (_isMouseOverSlider && !(maxedLeft || maxedRight))
            {
                KnobX += e.X - _lastX;
                _lastX = e.X;
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isMouseOverSlider = true;
                _lastX = e.X;
                KnobX = e.X - _knobSize.Width / 2;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int delta = -(e.Delta / 120);
            if (delta == -1) KnobX += 5;
            else if (delta == 1) KnobX -= 5;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            _knobRect.X = (int)((Size.Width - _knobRect.Width) * Percent + 0.5);
            UpdateGraphicsBuffer();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawSlider(_bufGraphics.Graphics);
            _bufGraphics.Render(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (Parent != null && BackColor == Color.Transparent)
            {
                Rectangle rect = new Rectangle(Left, Top, Width, Height);
                _bufGraphics.Graphics.TranslateTransform(-rect.X, -rect.Y);
                try
                {
                    using (PaintEventArgs pea = new PaintEventArgs(_bufGraphics.Graphics, rect))
                    {
                        pea.Graphics.SetClip(rect);
                        InvokePaintBackground(Parent, pea);
                        InvokePaint(Parent, pea);
                    }
                }
                finally
                {
                    _bufGraphics.Graphics.TranslateTransform(rect.X, rect.Y);
                }
            }
            else
            {
                using (SolidBrush backColor = new SolidBrush(this.BackColor))
                    _bufGraphics.Graphics.FillRectangle(backColor, ClientRectangle);
            }
        }
        #endregion

        #region Properties
        private BufferedGraphics _bufGraphics;
        private Rectangle _knobRect;
        private int _lastX;
        private bool _isMouseOverSlider;
        private Size _knobSize = new Size(14, 14);

        private int KnobX
        {
            get { return _knobRect.X; }
            set
            {
                if (value < 0) value = 0;
                if (value > this.Width - _knobRect.Width) value = this.Width - _knobRect.Width;
                Percent = (float)_knobRect.Left / (this.Width - _knobRect.Width);
                _knobRect.X = value;
            }
        }

        private float Percent
        {
            get { return (_value - _minimum) / (float)(_maximum - _minimum); }
            set
            {
                if (value > 1F) value = 1F;
                if (value < 0F) value = 0F;
                float val = (_maximum - _minimum) * value;
                Value = (int)(val + _minimum + 0.5);
                this.Invalidate();
            }
        }

        private Color _sliderLineColor = Color.Gainsboro;
        private Color _sliderFillColor = Color.LightCoral;
        private Color _thumbBorderColor = Color.LightCoral;
        private Color _thumbColor = Color.White;

        [Category("Custom Properties")]
        [Description("The slider back color")]
        [DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

        [Category("Custom Properties")]
        [Description("The slider knob border color")]
        public Color ThumbColor
        {
            get { return _thumbColor; }
            set { _thumbColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider knob color")]
        public Color ThumbBorderColor
        {
            get { return _thumbBorderColor; }
            set { _thumbBorderColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider line color")]
        public Color SliderLineColor
        {
            get { return _sliderLineColor; }
            set { _sliderLineColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider fill color")]
        public Color SliderFillColor
        {
            get { return _sliderFillColor; }
            set { _sliderFillColor = value; this.Invalidate(); }
        }

        private int _maximum = 100;
        [Description("The highest possible value")]
        [Category("Custom Properties")]
        [DefaultValue(100)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (value <= _minimum) throw new ArgumentOutOfRangeException("Value must be greater than Minimum");
                _maximum = value;
                if (_value > _maximum) Value = _maximum;
                UpdateKnobX();
                this.Invalidate();
            }
        }

        private int _minimum;
        [Description("The lowest possible value")]
        [Category("Custom Properties")]
        [DefaultValue(0)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (value >= _maximum) throw new ArgumentOutOfRangeException("Value must be less than Maximum");
                _minimum = value;
                if (_value < _minimum) Value = _minimum;
                UpdateKnobX();
                this.Invalidate();
            }
        }

        private int _value;
        [Description("The position of the slider")]
        [Category("Custom Properties")]
        [DefaultValue(50)]
        public int Value
        {
            get { return _value; }
            set
            {
                if (value < _minimum || value > _maximum) throw new ArgumentOutOfRangeException("value must be less than or equal to Maximum and greater than or equal to Minimum");
                bool changed = value != _value;
                if (changed)
                {
                    _value = value;
                    this.Invalidate();
					ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                UpdateKnobX();
            }
        }

        [Browsable(false)]
        public override Image BackgroundImage { get => base.BackgroundImage; set { } }
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout { get => base.BackgroundImageLayout; set { } }
        [Browsable(false)]
        public override Font Font { get => base.Font; set { } }
        [Browsable(false)]
        public override string Text { get => base.Text; set { } }
        [Browsable(false)]
        public override Color ForeColor { get => base.ForeColor; set { } }
        #endregion

        private void UpdateGraphicsBuffer()
        {
            _knobRect.Size = _knobSize;
            this.Height = _knobSize.Height;

            if (this.Width > 0 && this.Height > 0)
            {
                BufferedGraphicsContext context = BufferedGraphicsManager.Current;
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                _bufGraphics = context.Allocate(this.CreateGraphics(), this.ClientRectangle);
                IncreaseGraphicsQuality(_bufGraphics.Graphics);
            }
        }

        private void IncreaseGraphicsQuality(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        private void UpdateKnobX() => _knobRect.X = (int)((Size.Width - _knobRect.Width) * Percent + 0.5);

        protected virtual void DrawSlider(Graphics graphics)
        {
            int barHeight = (int)(this.Height / 5.0 + 0.5);
            int barY = this.Height / 2 - barHeight / 2;
            Rectangle rect = new Rectangle(2, barY, this.Width - 4, barHeight);
            using (SolidBrush lineBrush = new SolidBrush(_sliderLineColor))
            using (SolidBrush fillBrush = new SolidBrush(_sliderFillColor))
            using (SolidBrush thumbBorderBrush = new SolidBrush(_thumbBorderColor))
            using (SolidBrush thumbBrush = new SolidBrush(_thumbColor))
            {
                graphics.FillRectangle(lineBrush, rect);
                graphics.FillRectangle(fillBrush, 2, barY, KnobX + (_knobRect.Width - 4), barHeight);
                graphics.FillEllipse(thumbBorderBrush, KnobX, 0, _knobRect.Width, _knobRect.Height);
                graphics.FillEllipse(thumbBrush, KnobX + (_knobRect.Width - 8) / 2, (_knobRect.Height - 8) / 2, 8, 8);
            }
        }
    }
}
