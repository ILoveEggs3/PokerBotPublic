﻿using System.IO;
using Silence.Localization;
using Silence.Macro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsInputDLL;

namespace Silence
{
    public partial class FormMain : Form
    {
        private const string ConfigurationFilePath = "config.json";

        private readonly MacroRecorder _recorder = new MacroRecorder();
        private readonly MacroPlayer _player = new MacroPlayer();

        private readonly ConfigurationFile _config;
        private readonly LanguagePack _languages;

        public FormMain()
        {
            InitializeComponent();

            if (!File.Exists(ConfigurationFilePath))
                new ConfigurationFile().Save(ConfigurationFilePath);
            _config = ConfigurationFile.FromFile(ConfigurationFilePath);

            _languages = new LanguagePack(@"lang");
            _languages.SelectLanguage(_config.LanguageCode);
        }

        private void recordControlButton_Click(object sender, EventArgs e)
        {
            // Confirm action.
            if (_recorder.CurrentMacro != null && _recorder.CurrentMacro.Events.Length > 0)
            {
                var result = MessageBox.Show(_languages.GetLocalizedString("confirm_append_message"),
                    _languages.GetLocalizedString("confirm_append_title"), MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    _recorder.Clear();
                else if (result == DialogResult.Cancel)
                    return;
            }

            // Begin recording.
            _recorder.StartRecording();
        }

        private void stopControlButton_Click(object sender, EventArgs e)
        {
            // Stop recording.
            _recorder.StopRecording();
        }

        private void playControlButton_Click(object sender, EventArgs e)
        {
            // Load and play macro.
            _player.LoadMacro(_recorder.CurrentMacro);
            _player.PlayMacroAsync();
        }

        private void clearControlButton_Click(object sender, EventArgs e)
        {
            // Confirm action.
            if (_recorder.CurrentMacro != null && _recorder.CurrentMacro.Events.Length > 0)
            {
                var result = MessageBox.Show(_languages.GetLocalizedString("confirm_clear_message"),
                    _languages.GetLocalizedString("confirm_clear_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    _recorder.Clear();
            }
        }

        private void openControlButton_Click(object sender, EventArgs e)
        {
            // Confirm action.
            if (_recorder.CurrentMacro != null && _recorder.CurrentMacro.Events.Length > 0)
            {
                var result = MessageBox.Show(_languages.GetLocalizedString("confirm_open_message"),
                    _languages.GetLocalizedString("confirm_clear_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }

            // Browse for file
            var dialog = new OpenFileDialog
            {
                Title = _languages.GetLocalizedString("dialog_open_macro_title"),
                Filter = _languages.GetLocalizedString("dialog_open_macro_filter")
            };

            // Load macro into recorder.
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var loadedMacro = new Macro.Macro();
                loadedMacro.LoadFromFile(dialog.FileName);
                _recorder.LoadMacro(loadedMacro);
            }
        }
        static int ind = 1000;
        static Random rnd = new Random();
        private void saveControlButton_Click(object sender, EventArgs e)
        {
            // Check there is a macro to save.
            /*if (!(_recorder.CurrentMacro == null || _recorder.CurrentMacro.Events.Length == 0))
            {
                _recorder.StopRecording();
                string filename = String.Format(@"C:\Users\beao3002\Desktop\test\mouseMovement{0}.hush", ind++);
                _recorder.CurrentMacro.Save(filename);
                _recorder.Clear();
            }
            */
            Location = new Point(rnd.Next(0, 1200), rnd.Next(100, 800));
            Point targetArea = new Point(Location.X + this.saveControlButton.Location.X + saveControlButton.Width / 2,
                Location.Y + saveControlButton.Location.Y + saveControlButton.Height / 2 + 30);
            /*string directoryPath = @"C:\Users\beao3002\Desktop\test";
            string[] files = Directory.GetFiles(directoryPath);
            int randInd = rnd.Next(0, files.Length);
            string filePath = files[randInd];
            var loadedMacro = new Macro.Macro();
            loadedMacro.LoadFromFile(filePath);
            //_recorder.LoadMacro(loadedMacro);
            //_recorder.StartRecording();
            _player.LoadMacro(loadedMacro);
            _player.PlayMacroAsync();*/
            CMouseControllerSilence qwe = new CMouseControllerSilence();
            qwe.MoveMouseFromCurrentLocation(targetArea);
            
        }

        private void loopControlButton_Click(object sender, EventArgs e)
        {
            // Set number of repetitions on player.
            var dialog = new RepetitionsDialog { Repetitions = _player.Repetitions };
            if (dialog.ShowDialog() == DialogResult.OK)
                _player.Repetitions = dialog.Repetitions;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Load theme color for buttons.
            foreach (var control in panel1.Controls)
            {
                var button = (ControlButton)control;
                button.MouseOutBackgroundColor = _config.ThemeColor.ToColor();
                button.MouseOverBackgroundColor = _config.ThemeColor.ToColor(32);
                button.MouseDownBackgroundColor = _config.ThemeColor.ToColor(-32);
            }
        }
    }
}
