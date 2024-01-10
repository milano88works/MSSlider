using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace milano88.UI.Controls
{
    [DefaultEvent("ValueChanged")]
    public class MSSliderVertical : Control
    {
        public MSSliderVertical()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Height = 100;
            Value = 50;
            this.BackColor = Color.Transparent;
            UpdateGraphicsBuffer();
        }

        #region Default Event
        [Description("Occurs when the scroll percentage is changed")]
        public event EventHandler ValueChanged;
        #endregion

        #region Properties
        private BufferedGraphics _bufGraphics;
        private Rectangle _knobRect;
        private int _lastY;
        private bool _isMouseOverSlider;
        private Size _knobSize = new Size(14, 14);

        private int KnobY
        {
            get { return _knobRect.Y; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > this.Height - _knobRect.Height)
                    value = this.Height - _knobRect.Height;
                Percent = (float)_knobRect.Top / (this.Height - _knobRect.Height);
                _knobRect.Y = value;
            }
        }

        private float Percent
        {
            get { return (_value - _minimum) / (float)(_maximum - _minimum); }
            set
            {
                if (value > 1) value = 1;
                if (value < 0) value = 0;
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
        [DefaultValue(typeof(Color), "White")]
        public Color ThumbColor
        {
            get { return _thumbColor; }
            set { _thumbColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider knob color")]
        [DefaultValue(typeof(Color), "LightCoral")]
        public Color ThumbBorderColor
        {
            get { return _thumbBorderColor; }
            set { _thumbBorderColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider line color")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color SliderLineColor
        {
            get { return _sliderLineColor; }
            set { _sliderLineColor = value; this.Invalidate(); }
        }

        [Category("Custom Properties")]
        [Description("The slider fill color")]
        [DefaultValue(typeof(Color), "LightCoral")]
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
                if (value <= _minimum)
                    throw new ArgumentOutOfRangeException("Value must be greater than Minimum");

                _maximum = value;

                if (_value > _maximum)
                    Value = _maximum;

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
                if (value >= _maximum)
                    throw new ArgumentOutOfRangeException("Value must be less than Maximum");

                _minimum = value;

                if (_value < _minimum)
                    Value = _minimum;

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
                if (value < _minimum || value > _maximum)
                    throw new ArgumentOutOfRangeException("value must be less than or equal to Maximum and greater than or equal to Minimum");

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
            bool maxedBottom = (this.Height - e.Y) < 0 && Percent == 0f;
            bool maxedTop = (this.Height - e.Y) > this.Height && Percent == 1f;
            if (_isMouseOverSlider && !(maxedBottom || maxedTop))
            {
                KnobY += _lastY - e.Y;
                _lastY = e.Y;
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _lastY = e.Y;
                _isMouseOverSlider = true;
                KnobY = this.Height - e.Y - _knobSize.Width / 2;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int delta = -(e.Delta / 120);
            if (delta == -1)
                KnobY += 5;
            else if (delta == 1)
                KnobY -= 5;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            _knobRect.Y = (int)((Size.Height - _knobRect.Height) * Percent + 0.5);
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

        private void UpdateGraphicsBuffer()
        {
            _knobRect.Size = _knobSize;
            this.Width = _knobSize.Width;

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

        private void UpdateKnobX() => _knobRect.Y = (int)((Size.Height - _knobRect.Height) * Percent + 0.5);

        protected virtual void DrawSlider(Graphics graphics)
        {
            int barWidth = (int)(this.Width / 5.0 + 0.5);
            int barX = this.Width / 2 - barWidth / 2;
            Rectangle rect = new Rectangle(barX, 2, barWidth, this.Height - 4);
            using (SolidBrush lineBrush = new SolidBrush(_sliderLineColor))
            using (SolidBrush fillBrush = new SolidBrush(_sliderFillColor))
            using (SolidBrush thumbBorderBrush = new SolidBrush(_thumbBorderColor))
            using (SolidBrush thumbBrush = new SolidBrush(_thumbColor))
            {
                graphics.FillRectangle(lineBrush, rect);
                graphics.FillRectangle(fillBrush, barX, this.Height - KnobY, barWidth, KnobY - 2);
                int thumbY = this.Height - _knobRect.Height - KnobY;
                graphics.FillEllipse(thumbBorderBrush, 0, thumbY, _knobRect.Width, _knobRect.Height);
                graphics.FillEllipse(thumbBrush, (_knobRect.Width - 8) / 2, thumbY + (_knobRect.Height - 8) / 2, 8, 8);
            }
        }
    }
}
