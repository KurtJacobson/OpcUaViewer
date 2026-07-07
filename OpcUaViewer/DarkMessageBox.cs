using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpcUaViewer
{
    /// <summary>
    /// Dark-themed replacement for MessageBox.Show that matches the industrial HMI style.
    /// </summary>
    internal static class DarkMessageBox
    {
        private static readonly Color BgColor      = Color.FromArgb(52, 52, 52);
        private static readonly Color TitleBg      = Color.FromArgb(36, 36, 36);
        private static readonly Color ButtonBarBg  = Color.FromArgb(42, 42, 42);
        private static readonly Color BorderColor  = Color.FromArgb(80, 80, 80);
        private static readonly Color TextColor    = Color.FromArgb(230, 230, 230);
        private static readonly Color AccentColor  = Color.FromArgb(255, 140, 0);
        private static readonly Color DangerColor  = Color.FromArgb(180, 50, 50);
        private static readonly Color ButtonGray   = Color.FromArgb(70, 70, 70);

        public static DialogResult Show(
            IWin32Window owner,
            string text,
            string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var dlg = new DarkDialog(text, caption, buttons, icon);
            return owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
        }

        public static DialogResult Show(
            string text,
            string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None)
            => Show(null, text, caption, buttons, icon);

        // ── inner form ─────────────────────────────────────────────────────────

        private sealed class DarkDialog : Form
        {
            private Panel  _buttonBar;

            internal DarkDialog(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition   = FormStartPosition.CenterParent;
                BackColor       = BgColor;
                Font            = new Font("Segoe UI", 11F);
                ShowInTaskbar   = false;
                KeyPreview      = true;
                KeyDown        += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
                };
                Paint += (s, e) =>
                {
                    using var pen = new Pen(BorderColor, 2);
                    e.Graphics.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
                };

                // ── title bar ──────────────────────────────────────────────────
                var titleBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = TitleBg };
                var titleLbl = new Label
                {
                    Text      = caption,
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.FromArgb(210, 210, 210),
                    Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(16, 0, 0, 0)
                };
                titleBar.Controls.Add(titleLbl);

                var divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = BorderColor };

                // ── body ───────────────────────────────────────────────────────
                bool hasIcon = icon != MessageBoxIcon.None;
                (string sym, Color symColor) = icon switch
                {
                    MessageBoxIcon.Error       => ("✕", Color.FromArgb(230, 80, 80)),
                    MessageBoxIcon.Warning     => ("⚠", Color.FromArgb(255, 200, 0)),
                    MessageBoxIcon.Information => ("ℹ", Color.FromArgb(80, 170, 230)),
                    MessageBoxIcon.Question    => ("?", Color.FromArgb(80, 170, 230)),
                    _                          => ("",  Color.Transparent)
                };

                const int maxTextW = 540;
                const int hPad     = 24;
                int iconAreaW      = hasIcon ? 60 : 0;
                int textW          = maxTextW - iconAreaW;

                var msgFont  = new Font("Segoe UI", 11.5F);
                var textSize = TextRenderer.MeasureText(text, msgFont,
                    new Size(textW, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

                int bodyH = Math.Max(textSize.Height + 32, hasIcon ? 72 : 0);
                int formW = maxTextW + hPad * 2;
                int formH = 44 + 1 + bodyH + 72;   // title + divider + body + buttonbar

                var body = new Panel
                {
                    Location  = new Point(0, 45),
                    Size      = new Size(formW, bodyH),
                    BackColor = BgColor
                };

                if (hasIcon)
                {
                    var iconLbl = new Label
                    {
                        Text      = sym,
                        ForeColor = symColor,
                        Font      = new Font("Segoe UI", 24F),
                        TextAlign = ContentAlignment.TopCenter,
                        Location  = new Point(hPad, (bodyH - 36) / 2),
                        Size      = new Size(40, 40),
                        BackColor = Color.Transparent
                    };
                    body.Controls.Add(iconLbl);
                }

                var msgLbl = new Label
                {
                    Text      = text,
                    ForeColor = TextColor,
                    Font      = msgFont,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location  = new Point(hPad + iconAreaW, 0),
                    Size      = new Size(textW + (hasIcon ? 0 : hPad), bodyH),
                    BackColor = Color.Transparent
                };
                body.Controls.Add(msgLbl);

                // ── button bar ─────────────────────────────────────────────────
                _buttonBar = new Panel
                {
                    Location  = new Point(0, 45 + bodyH),
                    Size      = new Size(formW, 72),
                    BackColor = ButtonBarBg
                };

                var btnDivider = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 1,
                    BackColor = BorderColor
                };
                _buttonBar.Controls.Add(btnDivider);

                AddButtons(buttons, formW);

                ClientSize = new Size(formW, formH);
                Controls.Add(titleBar);
                Controls.Add(divider);
                Controls.Add(body);
                Controls.Add(_buttonBar);
            }

            private void AddButtons(MessageBoxButtons buttons, int formW)
            {
                const int btnW = 150, btnH = 46, gap = 10, rightPad = 16;
                int y = (72 - btnH) / 2 + 1;

                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        PlaceButton("OK", DialogResult.OK, AccentColor, true,
                            formW - rightPad - btnW, y, btnW, btnH);
                        break;
                    case MessageBoxButtons.OKCancel:
                        PlaceButton("Cancel", DialogResult.Cancel, ButtonGray, false,
                            formW - rightPad - btnW, y, btnW, btnH);
                        PlaceButton("OK", DialogResult.OK, AccentColor, true,
                            formW - rightPad - btnW * 2 - gap, y, btnW, btnH);
                        break;
                    case MessageBoxButtons.YesNo:
                        PlaceButton("No", DialogResult.No, DangerColor, false,
                            formW - rightPad - btnW, y, btnW, btnH);
                        PlaceButton("Yes", DialogResult.Yes, AccentColor, true,
                            formW - rightPad - btnW * 2 - gap, y, btnW, btnH);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        PlaceButton("Cancel", DialogResult.Cancel, ButtonGray, false,
                            formW - rightPad - btnW, y, btnW, btnH);
                        PlaceButton("No", DialogResult.No, DangerColor, false,
                            formW - rightPad - btnW * 2 - gap, y, btnW, btnH);
                        PlaceButton("Yes", DialogResult.Yes, AccentColor, true,
                            formW - rightPad - btnW * 3 - gap * 2, y, btnW, btnH);
                        break;
                    default:
                        PlaceButton("OK", DialogResult.OK, AccentColor, true,
                            formW - rightPad - btnW, y, btnW, btnH);
                        break;
                }
            }

            private void PlaceButton(string text, DialogResult result, Color bg,
                bool isDefault, int x, int y, int w, int h)
            {
                var btn = new Button
                {
                    Text                    = text,
                    DialogResult            = result,
                    BackColor               = bg,
                    ForeColor               = Color.White,
                    FlatStyle               = FlatStyle.Flat,
                    Font                    = new Font("Segoe UI", 11F, FontStyle.Bold),
                    UseVisualStyleBackColor = false,
                    Location                = new Point(x, y),
                    Size                    = new Size(w, h)
                };
                btn.FlatAppearance.BorderSize = 0;
                if (isDefault) AcceptButton = btn;
                _buttonBar.Controls.Add(btn);
            }
        }
    }
}
