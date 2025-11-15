using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatboxApp {
    public class ChatForm : Form {
        private readonly ChatSender chatSender;
        private readonly TextBox inputBox;
        private readonly TextBox endpointBox;
        private readonly Button submitButton;
        private readonly Button clearButton;
        private readonly CheckBox autoSubmitCheckBox;
        private readonly CheckBox clearOnSendCheckBox;
        private readonly CheckBox alwaysOnTopCheckBox;

        public ChatForm() {
            chatSender = new ChatSender();
            Text = "VRCChatbox";
            ClientSize = new Size(720, 240);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowIcon = false;
            MaximizeBox = false;
            Controls.Add(new TableLayoutPanel {
                Dock = DockStyle.Fill,
                AutoSize = false,
                ColumnCount = 1,
                RowCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 100F),
                    new RowStyle(SizeType.AutoSize),
                },
                Controls = {
                    (inputBox = new TextBox {
                        Dock = DockStyle.Fill,
                        MaxLength = 144,
                        WordWrap = true,
                        Multiline = true,
                        AcceptsTab = true,
                        ScrollBars = ScrollBars.Vertical,
                    }),
                    new TableLayoutPanel {
                        Dock = DockStyle.Fill,
                        Height = 50,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        RowCount = 1,
                        ColumnCount = 6,
                        ColumnStyles = {
                            new ColumnStyle(SizeType.Percent, 100F),
                            new ColumnStyle(SizeType.AutoSize),
                            new ColumnStyle(SizeType.AutoSize),
                            new ColumnStyle(SizeType.AutoSize),
                            new ColumnStyle(SizeType.AutoSize),
                            new ColumnStyle(SizeType.AutoSize),
                        },
                        Controls = {
                            (endpointBox = new TextBox {
                                Dock = DockStyle.Fill,
                                Text = chatSender.Destination,
                                PlaceholderText = "Destination (IP:Port)",
                                ImeMode = ImeMode.Off,
                            }),
                            (alwaysOnTopCheckBox = new CheckBox {
                                Text = "Always On Top",
                                AutoSize = true,
                                Dock = DockStyle.Fill,
                                FlatStyle = FlatStyle.System,
                                Checked = TopMost,
                            }),
                            (autoSubmitCheckBox = new CheckBox {
                                Text = "Auto Send",
                                AutoSize = true,
                                Dock = DockStyle.Fill,
                                FlatStyle = FlatStyle.System,
                            }),
                            (clearOnSendCheckBox = new CheckBox {
                                Text = "Clear On Send",
                                AutoSize = true,
                                Dock = DockStyle.Fill,
                                FlatStyle = FlatStyle.System,
                                Checked = true,
                            }),
                            (submitButton = new Button {
                                Text = "Send",
                                AutoSize = true,
                                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                Dock = DockStyle.Fill,
                                FlatStyle = FlatStyle.System,
                            }),
                            (clearButton = new Button {
                                Text = "Clear",
                                AutoSize = true,
                                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                Dock = DockStyle.Fill,
                                FlatStyle = FlatStyle.System,
                            }),
                        },
                    },
                },
            });
            AcceptButton = submitButton;
            CancelButton = clearButton;
            inputBox.GotFocus += OnInputBoxFocus;
            inputBox.LostFocus += OnInputBoxFocusLost;
            inputBox.TextChanged += OnInputBoxTextChanged;
            endpointBox.Enter += OnEndPointBoxEnter;
            submitButton.Click += OnSubmitButtonClick;
            clearButton.Click += OnClearButtonClick;
            autoSubmitCheckBox.CheckedChanged += OnAutoSubmitCheckBoxCheckedChanged;
            alwaysOnTopCheckBox.CheckedChanged += OnAlwaysOnTopCheckBoxCheckedChanged;
        }

        private void OnInputBoxTextChanged(object? sender, EventArgs e) {
            chatSender.Text = inputBox.Text;
            if (autoSubmitCheckBox.Checked) chatSender.SendText(false);
        }

        private void OnInputBoxFocus(object? sender, EventArgs e) {
            chatSender.ShouldSendTyping = true;
            chatSender.SendTypingNotification();
        }

        private void OnInputBoxFocusLost(object? sender, EventArgs e) {
            chatSender.ShouldSendTyping = false;
            chatSender.SendTypingNotification();
        }

        private void OnSubmitButtonClick(object? sender, EventArgs e) {
            chatSender.SendText();
            if (clearOnSendCheckBox.Checked) inputBox.Clear();
        }

        private void OnClearButtonClick(object? sender, EventArgs e) {
            inputBox.Clear();
            chatSender.Text = "";
            chatSender.SendText(false);
        }

        private void OnAutoSubmitCheckBoxCheckedChanged(object? sender, EventArgs e) {
            inputBox.AcceptsReturn = autoSubmitCheckBox.Checked;
        }

        private void OnAlwaysOnTopCheckBoxCheckedChanged(object? sender, EventArgs e) {
            TopMost = alwaysOnTopCheckBox.Checked;
        }

        private void OnEndPointBoxEnter(object? sender, EventArgs e) {
            if (!chatSender.TrySetDestination(endpointBox.Text)) endpointBox.Text = chatSender.Destination;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                chatSender.Dispose();
            }
        }
    }
}
