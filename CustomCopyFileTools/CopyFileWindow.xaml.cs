using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CustomCopyFileTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CopyFileWindow : Window
    {
        private readonly Configuration _config;
        public CopyFileWindow()
        {
            InitializeComponent();
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var originalPath = _config.AppSettings.Settings["originalPath"].Value;
            var targetPath = _config.AppSettings.Settings["targetPath"].Value;
            this.TbOriginalPath.Text = originalPath;
            this.TbtargetPath.Text = targetPath;
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
                MessageBox.Show("路径存在包含关系，无法复制", "温馨提示");
                return;
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {

            if (TbtargetPath.Text.Trim().Contains(TbOriginalPath.Text.Trim()))
            {
                MessageBox.Show("路径有问题，不能复制文件", "温馨提示");
                return;
            }

            if (!CopyDirectoty(TbOriginalPath.Text, TbtargetPath.Text, true))
            {
                MessageBox.Show("复制失败", "温馨提示");
                return;
            }
            else
            {
                MessageBox.Show("复制成功", "温馨提示");
                this.Close();
                return;
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
                foreach (var file in originalFiles)
                {
                    var flinfo = new FileInfo(file);
                    flinfo.CopyTo(targetPath + flinfo.Name, overWrite);
                }
                result = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                result = false;
            }
            return result;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
