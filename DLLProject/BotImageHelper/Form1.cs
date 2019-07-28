using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotImageHelper
{
    public partial class Form1 : Form
    {
        static int FFInd = 0;
        static int FFRefInd = 0;
        static int FFNbZipImages = 0;
        static int FFNbRefs = 0;
        static ConstantFileGenerator FFConstFileGenerator;
        public Form1()
        {
            FFConstFileGenerator = new ConstantFileGenerator();
            FFInd = 0;
            InitializeComponent();
            var fileList = Directory.EnumerateFiles(@"../../Templates/");
            foreach (var item in fileList)
            {
                string fileName = Path.GetFileName(item);
                drp_CreateNewFileConstant.Items.Add(fileName);
            }
            if (drp_CreateNewFileConstant.Items.Count > 0)
            {
                drp_CreateNewFileConstant.SelectedIndex = 0;
            }
            foreach (var item in ConstantFileGenerator.PRegionTypes)
            {
                cmb_FileType.Items.Add(item.Key);
            }
            cmb_FileType.SelectedIndex = cmb_FileType.Items.IndexOf(ConstantFileGenerator.RegionType.OverrrideRectangle);
        }

        private Bitmap getImageFromSource()
        {
            Bitmap bmpImg = new Bitmap(1, 1);
            if (txt_source.Text.EndsWith(".bmp"))
            {
                var files = Directory.GetFiles(Path.GetDirectoryName(txt_source.Text)).Where(x => x.EndsWith(".bmp")).ToList();
                txt_source.Text = files[FFInd];
                bmpImg = new Bitmap(txt_source.Text);
                //lbl_loadedImage.Text = Path.GetFileName(_source);
            }
            else if (txt_source.Text.EndsWith(".zip"))
            {
                string zipFile = Path.GetFullPath(txt_source.Text);
                if (!zipFile.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    zipFile += Path.DirectorySeparatorChar;
                using (ZipArchive archive = ZipFile.OpenRead(txt_source.Text))
                {
                    FFNbZipImages = archive.Entries.Count;
                    var qwe = archive.Entries[FFInd];
                    //lbl_loadedImage.Text = Path.GetFileName(qwe.FullName);
                    bmpImg = new Bitmap(qwe.Open());
                }
            }
            else
            {
                MessageBox.Show("Unrecognized source file type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return bmpImg;
        }


        private void cmd_openFile_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files(*.BMP;*.zip)|*.BMP;*.zip";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Title = "Select Images to crop";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txt_source.Text = openFileDialog1.FileName;
                if (txt_source.Text.EndsWith(".bmp"))
                {
                    var files = Directory.GetFiles(Path.GetDirectoryName(txt_source.Text)).Where(x => x.EndsWith(".bmp")).ToList();
                    FFNbZipImages = files.Count;
                    FFInd = files.IndexOf(txt_source.Text);
                }
                img_box.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateSamplePicture();
                btn_NextZipImg.Enabled = btn_PrevZipImg.Enabled = true;
                tgl_ZoomMode.Enabled = grp_Coord.Enabled = grp_Dimension.Enabled = true;
            }
        }

        private void cmd_openFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select the directory that you want to use as the default.";
            folderBrowserDialog1.ShowNewFolderButton = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.txt_destination.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void cmd_extract_Click(object sender, EventArgs e)
        {
            string sourceFiles = txt_source.Text;
            string destinationFolder = txt_destination.Text;

            Tuple<int, int> coord = new Tuple<int, int>((int)nud_X.Value, (int)nud_Y.Value);
            Tuple<int, int> dimension = new Tuple<int, int>((int)nud_Width.Value, (int)nud_Height.Value);
            try
            {
                string name = txt_gameType.Text + "_" + txt_platform.Text + "_" + txt_name.Text;
                int nbImg = ImageExtractor.cropImagesFromSources(sourceFiles, destinationFolder, name, coord, dimension);
                string message = String.Format("Successfully extracted {0} image to {1}.", nbImg, destinationFolder);
                MessageBox.Show(message, "Extraction completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Failed to extract images\nDo verify your inputs", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSamplePicture()
        {
            string source = txt_source.Text;
            Bitmap MyImage = getImageFromSource();
            nud_X.Maximum = nud_Width.Maximum = MyImage.Width;
            nud_Y.Maximum = nud_Height.Maximum = MyImage.Height;
            MyImage = MyImage.Clone(new Rectangle((int)nud_X.Value, (int)nud_Y.Value, (int)nud_Width.Value, (int)nud_Height.Value), MyImage.PixelFormat);
            if (img_box.Image != null)
                img_box.Image.Dispose();
            img_box.Image = (Image)MyImage;
        }

        private void nud_Y_ValueChanged(object sender, EventArgs e)
        {
            Bitmap bmpImg = getImageFromSource();
            nud_Y.Value = Math.Min(nud_Y.Value, bmpImg.Height - nud_Height.Value);
            UpdateSamplePicture();
        }

        private void nud_Width_ValueChanged(object sender, EventArgs e)
        {
            Bitmap bmpImg = getImageFromSource();
            nud_Width.Value = Math.Min(nud_Width.Value, bmpImg.Width - nud_X.Value);
            UpdateSamplePicture();
        }

        private void nud_X_ValueChanged(object sender, EventArgs e)
        {
            Bitmap bmpImg = getImageFromSource();
            nud_X.Value = Math.Min(nud_X.Value, bmpImg.Width - nud_Width.Value);
            UpdateSamplePicture();
        }

        private void nud_Height_ValueChanged(object sender, EventArgs e)
        {
            Bitmap bmpImg = getImageFromSource();
            nud_Height.Value = Math.Min(nud_Height.Value, bmpImg.Height - nud_Y.Value);
            UpdateSamplePicture();
        }

        private void tgl_StretchMode_Click(object sender, EventArgs e)
        {
            if (img_box.SizeMode == PictureBoxSizeMode.Zoom)
            {
                img_box.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                img_box.SizeMode = PictureBoxSizeMode.Zoom;
            }

        }

        private void btn_2020_Click(object sender, EventArgs e)
        {
            nud_Width.Value = nud_Height.Value = 20;
        }

        private void btn_5050_Click(object sender, EventArgs e)
        {
            nud_Width.Value = nud_Height.Value = 50;
        }

        private void btn_CoordSelector_Click(object sender, EventArgs e)
        {

            Bitmap bmpImg = getImageFromSource();
            if (bmpImg.Height == 1)
            {
                return;
            }
            Form2 form = new Form2();

            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Image = bmpImg;
            form.Size = new Size(pictureBox.Image.Width, pictureBox.Image.Height + 30);
            //pictureBox.Size = new Size(pictureBox.Image.Width, pictureBox.Image.Height + 30);
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox.MouseClick += new MouseEventHandler(form.Form2_MouseDown);
            form.Controls.Add(pictureBox);

            form.ShowDialog();
            Point p;
            p = form.location;
            nud_X.Value = p.X;
            nud_Y.Value = p.Y;
        }

        private void btn_NextZipImg_Click(object sender, EventArgs e)
        {
            FFInd = ++FFInd % FFNbZipImages;
            UpdateSamplePicture();
        }

        private void btn_PrevZipImg_Click(object sender, EventArgs e)
        {
            FFInd = --FFInd < 0 ? FFNbZipImages - 1 : FFInd;
            UpdateSamplePicture();
        }

        private void UpdatelstCoords()
        {
            var regionList = FFConstFileGenerator.GetRegionList();
            lst_Coords.Items.Clear();
            foreach (var item in regionList)
            {
                lst_Coords.Items.Add(item.PName);
            }
            var str = FFConstFileGenerator.RegionListToString();
            txt_Preview.Text = str;
            FFConstFileGenerator.SaveToFile(txt_LoadConstantFile.Text);
        }

        private void LoadConstantFile(string fileName)
        {
            this.txt_LoadConstantFile.Text = fileName;
            FFConstFileGenerator.LoadConstantsFile(txt_LoadConstantFile.Text);
            UpdatelstCoords();
            cmd_AddRegion.Enabled = cmd_DeleteRegion.Enabled = cmd_SaveRegion.Enabled = txt_RegionName.Enabled = true;
        }

        private void cmd_LoadConstantFile_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor. 
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files(*.cs)|*.cs";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Title = "Select Constants File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadConstantFile(openFileDialog1.FileName);
            }
        }

        private void lst_Coords_SelectedIndexChanged(object sender, EventArgs e)
        {
            var regionList = FFConstFileGenerator.GetRegionList();
            string itemName = (string)lst_Coords.SelectedItem;

            foreach (var item in regionList)
            {
                if (item.PName == itemName)
                {
                    if (txt_source.Text.Length > 1)
                    {
                        UpdateSamplePicture();
                        nud_X.Value = item.PValues.X;
                        nud_Y.Value = item.PValues.Y;
                        nud_Width.Value = item.PValues.Width;
                        nud_Height.Value = item.PValues.Height;

                        UpdateSamplePicture();
                        txt_name.Text = (string)lst_Coords.Items[lst_Coords.SelectedIndex] + "_BMP";

                    }
                    txt_RegionName.Text = (string)lst_Coords.Items[lst_Coords.SelectedIndex];
                    break;
                }
            }
        }

        private Rectangle GetRectangleFromUserInputs()
        {
            Rectangle rec = new Rectangle((int)nud_X.Value, (int)nud_Y.Value, (int)nud_Width.Value, (int)nud_Height.Value);
            return rec;
        }

        private void cmd_AddRegion_Click(object sender, EventArgs e)
        {
            Rectangle rec = GetRectangleFromUserInputs();
            string regionName = txt_RegionName.Text;
            ConstantFileGenerator.RegionType type = (ConstantFileGenerator.RegionType)cmb_FileType.SelectedItem;

            int ret = FFConstFileGenerator.AddRegion(regionName, rec, type);
            if (ret == -2)
            {
                MessageBox.Show("A region with that name already exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (ret == -1)
            {
                MessageBox.Show("Invalid region Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (ret == 0)
            {
                UpdatelstCoords();
            }
        }

        private void cmd_DeleteRegion_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to delete this region?", "Delete region", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                FFConstFileGenerator.DeleteRegion((string)lst_Coords.SelectedItem);
                UpdatelstCoords();
            }
        }

        private void cmd_SaveRegion_Click(object sender, EventArgs e)
        {
            FFConstFileGenerator.UpdateRegion((string)lst_Coords.SelectedItem, GetRectangleFromUserInputs(), txt_RegionName.Text);
            UpdatelstCoords();
        }

        private void cmd_CreateNewFileConstant_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "cs files (*.cs)|*.cs";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    byte[] ba = File.ReadAllBytes(@"../../Templates/" + drp_CreateNewFileConstant.Items[drp_CreateNewFileConstant.SelectedIndex]);
                    myStream.Write(ba, 0, ba.Length);
                    myStream.Close();
                }
                LoadConstantFile(saveFileDialog1.FileName);
            }
        }

        private void cmb_FileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConstantFileGenerator.RegionType type = (ConstantFileGenerator.RegionType)cmb_FileType.SelectedItem;
            FFConstFileGenerator.SetType(type);
            UpdatelstCoords();
        }

        private Bitmap getReferenceImageFromSource()
        {
            Bitmap bmpImg = new Bitmap(1, 1);
            if (txt_loadReference.Text.EndsWith(".bmp"))
            {
                var files = Directory.GetFiles(Path.GetDirectoryName(txt_loadReference.Text)).Where(x => x.EndsWith(".bmp")).ToList();
                txt_loadReference.Text = files[FFRefInd];
                bmpImg = new Bitmap(txt_loadReference.Text);
                //lbl_loadedImage.Text = Path.GetFileName(_source);
            }
            else if (txt_loadReference.Text.EndsWith(".zip"))
            {
                string zipFile = Path.GetFullPath(txt_loadReference.Text);
                if (!zipFile.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    zipFile += Path.DirectorySeparatorChar;
                using (ZipArchive archive = ZipFile.OpenRead(txt_loadReference.Text))
                {
                    FFNbZipImages = archive.Entries.Count;
                    var qwe = archive.Entries[FFRefInd];
                    //lbl_loadedImage.Text = Path.GetFileName(qwe.FullName);
                    bmpImg = new Bitmap(qwe.Open());
                }
            }
            else
            {
                MessageBox.Show("Unrecognized source file type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return bmpImg;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files(*.BMP;*.zip)|*.BMP;*.zip";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Title = "Select Images to crop";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txt_loadReference.Text = openFileDialog1.FileName;
                if (txt_loadReference.Text.EndsWith(".bmp"))
                {
                    var files = Directory.GetFiles(Path.GetDirectoryName(txt_loadReference.Text)).Where(x => x.EndsWith(".bmp")).ToList();
                    FFNbRefs = files.Count;
                    FFRefInd = files.IndexOf(txt_loadReference.Text);
                }
                img_boxRef.SizeMode = PictureBoxSizeMode.Zoom;
                Bitmap MyImage = getReferenceImageFromSource();
                img_boxRef.Image = MyImage;
                cmd_nextReference.Enabled = cmd_PreviousReference.Enabled = cmd_zoomReference.Enabled = true;
            }
        }

        private void UpdateReferencePicture()
        {
            string source = txt_loadReference.Text;
            Bitmap MyImage = getReferenceImageFromSource();
            img_boxRef.Image = (Image)MyImage;
        }

        private void cmd_nextReference_Click(object sender, EventArgs e)
        {
            FFRefInd = ++FFRefInd % FFNbRefs;
            UpdateReferencePicture();
        }

        private void cmd_PreviousReference_Click(object sender, EventArgs e)
        {
            FFRefInd = --FFRefInd < 0 ? FFNbRefs - 1 : FFRefInd;
            UpdateReferencePicture();
        }

        private void cmd_zoomReference_Click(object sender, EventArgs e)
        {
            if (img_boxRef.SizeMode == PictureBoxSizeMode.Zoom)
            {
                img_boxRef.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                img_boxRef.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void CalculateOpenCLDifferences()
        {
            if (img_box.Image == null || img_boxRef.Image == null)
                return;
            var sample = new Bitmap(txt_source.Text);
            var coord = new Point((int)nud_X.Value, (int)nud_Y.Value);
            var reference = new Bitmap(img_boxRef.Image).Clone(new Rectangle(0, 0, Math.Min(img_box.Image.Width, img_boxRef.Image.Width), Math.Min(img_box.Image.Height, img_boxRef.Image.Height)), img_boxRef.Image.PixelFormat);

            var dist = OpenCL.OpenCLController.CalculateDistances(sample, coord, reference);
            txt_differences.Text = dist.ToString();
        }

        private void img_boxRef_Paint(object sender, PaintEventArgs e)
        {
            CalculateOpenCLDifferences();
        }

        private void img_box_Paint(object sender, PaintEventArgs e)
        {
            CalculateOpenCLDifferences();
        }
    }
}
