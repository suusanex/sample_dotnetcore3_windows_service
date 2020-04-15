using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;

namespace VSTOClientWord
{
    public partial class ThisAddIn
    {
        private ServiceToVSTOClient.IVSTOToService m_Client = ServiceToVSTOClient.VSTOToServiceFactory.CreateInstance();

        private async void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.DocumentChange += OnDocumentChange;

            await m_Client.ConnectAsync().ConfigureAwait(false);
        }

        private async void OnDocumentChange()
        {
            var filePath = Application.ActiveDocument.FullName;

            await m_Client.ServerCallTestAsync(filePath).ConfigureAwait(false);
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            m_Client?.Dispose();
        }

        #region VSTO で生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
