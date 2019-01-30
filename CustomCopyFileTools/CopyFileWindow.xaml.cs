using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Threading;
using Panuon.UI;

namespace CustomCopyFileTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CopyFileWindow
    {
        private readonly Configuration _config;

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        public CopyFileWindow()
        {
            InitializeComponent();
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var originalPath = _config.AppSettings.Settings["originalPath"].Value;
            var targetPath = _config.AppSettings.Settings["targetPath"].Value;
            TbOriginalPath.Text = originalPath;
            TbtargetPath.Text = targetPath;
        }

        private void BtnBrowserOriginalPath_Click(object sender, RoutedEventArgs e)
        {
            var chooseFolderDialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(TbOriginalPath.Text) && Directory.Exists(TbOriginalPath.Text))
            {
                chooseFolderDialog.SelectedPath = TbOriginalPath.Text;
            }
            if (chooseFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbOriginalPath.Text = chooseFolderDialog.SelectedPath;
                UpdateConfig("originalPath", TbOriginalPath.Text);
            }

        }

        private void BtnBrowserTargetPath_Click(object sender, RoutedEventArgs e)
        {
            var chooseFolderDialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(TbtargetPath.Text) && Directory.Exists(TbtargetPath.Text))
            {
                chooseFolderDialog.SelectedPath = TbtargetPath.Text;
            }
            if (chooseFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbtargetPath.Text = chooseFolderDialog.SelectedPath;
                UpdateConfig("targetPath", TbtargetPath.Text);
            }
        }

        private void UpdateConfig(string key, string value)
        {
            _config.AppSettings.Settings[key].Value = value;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void CheckPath()
        {
            if (TbtargetPath.Text.Trim().Contains(TbOriginalPath.Text.Trim()))
            {
                PUMessageBox.ShowConfirm("路径存在包含关系，无法复制", "温馨提示", buttons: Buttons.OK);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {

            if (TbtargetPath.Text.Trim().Contains(TbOriginalPath.Text.Trim()))
            {
                PUMessageBox.ShowConfirm("路径有问题，不能复制文件", "温馨提示", Buttons.Sure, animateStyle: AnimationStyles.Fade);
                return;
            }
            if (!CopyDirectoty(TbOriginalPath.Text, TbtargetPath.Text, true))
            {
                PUMessageBox.ShowConfirm("复制失败", "温馨提示", Buttons.Sure, animateStyle: AnimationStyles.Fade);
            }
            else
            {
                PUMessageBox.ShowConfirm("复制成功", "温馨提示", Buttons.Sure, animateStyle: AnimationStyles.Fade);//.Show("复制成功", "温馨提示");
                Close();
            }
        }

        private bool CopyDirectoty(string sourcePath, string targetPath, bool overWrite)
        {
            bool result;
            try
            {
                sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
                targetPath = targetPath.EndsWith(@"\") ? targetPath : targetPath + @"\";
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                var orginalDirectories = Directory.GetDirectories(sourcePath);
                foreach (var directory in orginalDirectories)
                {
                    var driInfo = new DirectoryInfo(directory);
                    if (!CopyDirectoty(directory, targetPath + driInfo.Name, overWrite))
                    {
                        return false;
                    }
                }
                var originalFiles = Directory.GetFiles(sourcePath);

                //ProgressBar.IsPercentShow = true;
                //ProgressBar.
                ProgressBar.Maximum = originalFiles.Length;
                ProgressBar.Visibility = Visibility.Visible;
                LbPercent.Visibility = Visibility.Visible;
                UpdateProgressBarDelegate updateProgressBaDelegate = ProgressBar.SetValue;
                var count = 0.0;
                foreach (var file in originalFiles)
                {
                    var flinfo = new FileInfo(file);
                    flinfo.CopyTo(targetPath + flinfo.Name, overWrite);
                    count++;
                    Dispatcher.Invoke(updateProgressBaDelegate, DispatcherPriority.Background, RangeBase.ValueProperty, count);
                    LbPercent.Content = $"{Math.Round(count * 100 / originalFiles.Length, 4)}%";
                }
                ProgressBar.Visibility = Visibility.Hidden;
                LbPercent.Visibility = Visibility.Hidden;
                result = true;
            }
            catch (Exception e)
            {
                PUMessageBox.ShowConfirm(e.Message, buttons: Buttons.OK, animateStyle: AnimationStyles.Fade);
                result = false;
            }
            return result;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
