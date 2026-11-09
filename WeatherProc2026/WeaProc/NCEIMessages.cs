using System;
using System.Windows.Forms;

namespace NCEIData
{
    public class NCEImessage
    {
        public void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
