using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar.Controls;

namespace NoDev.Horizon.Controls
{
    internal class ControlLoader : UserControl
    {
        private readonly Control _control;
        private readonly Image _controlImage;

        private readonly CircularProgress _progress;

        private bool _continuousLoad = true;

        internal static async Task<dynamic> RunAsync(Control control, Func<ControlLoader, dynamic> method, int alpha = 50)
        {
            var cl = new ControlLoader(control, alpha);

            var timer = new Stopwatch();
            timer.Start();

            try
            {
                var x = await Task.Run(() => {
                    try
                    {
                        return method(cl);
                    }
                    catch (Exception e)
                    {
                        return e;
                    }
                });

                var exception = x as Exception;
                if (exception != null)
                    throw exception;

                return x;
            }
            finally
            {
                timer.Stop();
                if (timer.ElapsedMilliseconds > 2000)
                    ThreadPool.QueueUserWorkItem(cl.ShrinkProgress);
                else
                    cl.Close();
            }
        }

        private void ShrinkProgress(object threadObj)
        {
            this._continuousLoad = true;

            int w2 = Width / 2, h2 = Height / 2;

            while (this._progress.Width > 1)
            {
                this.Invoke(() =>
                {
                    _progress.Size = new Size(_progress.Width - 8, _progress.Height - 8);

                    var pw2 = _progress.Width / 2;

                    _progress.Location = new Point(w2 - pw2, h2 - pw2);
                });

                Thread.Sleep(4);
            }

            Invoke((Action)Close);
        }

        internal int Value
        {
            get
            {
                return this._progress.Value;
            }
            set
            {
                this._continuousLoad = false;
                this._progress.Invoke(() => this._progress.Value = value);
            }
        }

        internal int Maximum
        {
            get
            {
                return this._progress.Maximum;
            }
            set
            {
                this._progress.Invoke(() => this._progress.Maximum = value);
            }
        }

        internal ControlLoader(Control control, int alpha = 50)
        {
            this._control = control;
            this._controlImage = control.ToBitmap();

            using (var g = Graphics.FromImage(this._controlImage))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(alpha, Color.Black)), new Rectangle(0, 0, control.Width, control.Height));
                g.Flush();
                g.Save();
            }

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            this.Paint += ControlLoader_Paint;
            this.MouseDown += ControlLoader_MouseDown;

            this.Size = control.Size;

            var progress = new CircularProgress();

            progress.MouseDown += ControlLoader_MouseDown;
            progress.FocusCuesEnabled = false;
            progress.BackColor = Color.Transparent;

            int width = Width / 3;

            if (width >= Height)
                width = (int)(Height / 1.5);

            progress.Size = new Size(width, width);

            var w2 = progress.Width / 2;

            progress.Location = new Point(Width / 2 - w2, Height / 2 - w2);

            this.Controls.Add(progress);

            this._progress = progress;

            control.Controls.Add(this);

            this.BringToFront();

            Task.Run((Action)ContinuousLoad);
        }

        private void ControlLoader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(this.Parent.Handle, 0xa1, 2, 0);
        }

        private void ContinuousLoad()
        {
            while (this.Parent != null && _continuousLoad)
            {
                Thread.Sleep(7);
                if (this._progress.Value == this._progress.Maximum)
                    this._progress.Value = this._progress.Minimum;
                else
                    this._progress.Value++;
            }
        }

        internal void Close()
        {
            this._control.Controls.Remove(this);
        }

        private void ControlLoader_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(this._controlImage, Point.Empty);
        }
    }
}
