using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using AS.HDFql;

namespace WeaHDFTest
{
    public partial class HDF5Test : Form
    {
        private string HDF5file;
        public HDF5Test()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string ext = ".h5";
            string filter = "HDF5 database (*.h5)|*.h5|All files (*.*)|*.*";
            //string filter = "(*.h5)|*.h5";
            string sFile = string.Empty;

            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                openFD.AddExtension = true;
                openFD.CheckFileExists = false;
                openFD.DefaultExt = ext;
                openFD.InitialDirectory = Application.StartupPath;
                openFD.Filter = filter;
                openFD.FilterIndex = 1;
                openFD.RestoreDirectory = true;
                openFD.Title = "Select or Create new output HDF5 database ...";
                if (openFD.ShowDialog() == DialogResult.OK)
                {
                    sFile = openFD.FileName;
                    txtHDF5.Text = sFile;
                    HDF5file = sFile;
                }
                else
                {
                    sFile = string.Empty;
                    HDF5file = sFile;
                    return;
                }

                if (!File.Exists(sFile))
                {
					File.Create(sFile);
				}
				HDF5file = sFile;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
			TestHDFql(HDF5file);
			//this.Close();
            //this.Dispose();
        }

        private void txtHDF5_TextChanged(object sender, EventArgs e)
        {
			string sfile = txtHDF5.Text;

			if (!File.Exists(sfile))
            {
				if (!sfile.Contains(".h5"))
					sfile = string.Concat(sfile, ".h5");
				File.Create(sfile);
				//HDFql.Execute("CREATE FILE " + sfile);
				HDF5file = sfile;
			}
        }

        private void TestHDFql(string h5file)
        {
			// declare variables
			HDFqlCursor myCursor;
			int[,] values;
			int x;
			int y;

			// display HDFql version in use
			System.Console.WriteLine("HDFql version: {0}", HDFql.Version);

			// create an HDF5 file named "example.h5" and use (i.e. open) it
			string s = "CREATE FILE " + h5file;
			HDFql.Execute(s);
			s = "USE FILE " + h5file;
			HDFql.Execute(s);

			// show (i.e. get) HDF5 file currently in use and populate HDFql default cursor with it
			HDFql.Execute("SHOW USE FILE");

			// display HDF5 file currently in use
			HDFql.CursorFirst();
			Debug.WriteLine("File in use: {0}", HDFql.CursorGetChar());

			// create an attribute named "example_attribute" of data type float with an initial value of 12.4
			HDFql.Execute("CREATE ATTRIBUTE example_attribute AS FLOAT VALUES(12.4)");

			// select (i.e. read) data from attribute "example_attribute" and populate HDFql default cursor with it
			HDFql.Execute("SELECT FROM example_attribute");

			// display value of attribute "example_attribute"
			HDFql.CursorFirst();
			Debug.WriteLine("Attribute value: {0}", HDFql.CursorGetFloat());

			// create a dataset named "example_dataset" of data type int of two dimensions (size 3x2)
			HDFql.Execute("CREATE DATASET example_dataset AS INT(3, 2)");

			// create variable "values" and populate it with certain values
			values = new int[3, 2];
			for (x = 0; x < 3; x++)
			{
				for (y = 0; y < 2; y++)
				{
					values[x, y] = x * 2 + y + 1;
				}
			}

			// register variable "values" for subsequent use (by HDFql)
			HDFql.VariableRegister(values);

			// insert (i.e. write) values from variable "values" into dataset "example_dataset"
			HDFql.Execute("INSERT INTO example_dataset VALUES FROM MEMORY " + HDFql.VariableGetNumber(values));

			// populate variable "values" with zeros (i.e. reset variable)
			//for (x = 0; x < 3; x++)
			//{
			//	for (y = 0; y < 2; y++)
			//	{
			//		values[x, y] = x*y;
			//	}
			//}

			// select (i.e. read) data from dataset "example_dataset" and populate variable "values" with it
			HDFql.Execute("SELECT FROM example_dataset INTO MEMORY " + HDFql.VariableGetNumber(values));

			// unregister variable "values" as it is no longer used/needed (by HDFql)
			HDFql.VariableUnregister(values);

			// display content of variable "values"
			Debug.WriteLine("Dataset value (through variable):");
			for (x = 0; x < 3; x++)
			{
				for (y = 0; y < 2; y++)
				{
					Debug.WriteLine(values[x, y]);
				}
			}

			// another way to select (i.e. read) data from dataset "example_dataset" using HDFql default cursor
			HDFql.Execute("SELECT FROM example_dataset");

			// display content of HDFql default cursor
			Debug.WriteLine("Dataset value (through cursor):");
			while (HDFql.CursorNext() == HDFql.Success)
			{
				System.Console.WriteLine(HDFql.CursorGetInt());
			}

			// create cursor "myCursor" and use it
			myCursor = new HDFqlCursor();
			HDFql.CursorUse(myCursor);

			// show (i.e. get) size (in bytes) of dataset "example_dataset" and populate cursor "myCursor" with it
			HDFql.Execute("SHOW SIZE example_dataset");

			// display content of cursor "myCursor"
			HDFql.CursorFirst();
			Debug.WriteLine("Dataset size (in bytes): {0}", HDFql.CursorGetBigInt());

		}
	}
}
