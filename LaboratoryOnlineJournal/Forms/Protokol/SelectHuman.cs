using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LaboratoryOnlineJournal.Forms.Protokol;

namespace LaboratoryOnlineJournal.Forms.Protokol
{
    public partial class SelectHuman : Form
    {
        public SelectHuman()
        {
            InitializeComponent();
            G.PodrPpl.QUERRY().SHOW.WHERE.C(C.PodrPpl.Podr, 4).OR.
                                          C(C.PodrPpl.Podr, 9).OR.
                                          C(C.PodrPpl.Podr, 17).OR.
                                          C(C.PodrPpl.Podr, 18).OR.
                                          C(C.PodrPpl.Podr, 92).OR.
                                          C(C.PodrPpl.Podr, 19).OR.
                                          C(C.PodrPpl.Podr, 73).OR.
                                          C(C.PodrPpl.Podr, 39).OR.
                                          C(C.PodrPpl.Podr, 27).OR.
                                          C(C.PodrPpl.Podr, 34).DO();
            int count = G.PodrPpl.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                _selectHuman.Items.Add(G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.Podr, C.Podr.ShrName) + " | " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name1)
                    + " " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name2) + " " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name3)
                                       + " | " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.Prfssn, C.Prfssn.Name));
                _selectSubs.Items.Add(G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.Podr, C.Podr.ShrName) + " | " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name1)
                    + " " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name2) + " " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.name3)
                                       + " | " + G.PodrPpl.Rows.Get_UnShow<string>(i, C.PodrPpl.People, C.People.Prfssn, C.Prfssn.Name));
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (_selectHuman.SelectedIndex == -1)
            {
                MessageBox.Show("Требуется выбрать пробоотборщика!");
            } else {
                string [] _pSubs = _selectSubs.Items[_selectHuman.SelectedIndex].ToString().Split('|');
                string [] _pHuman = _selectHuman.Items[_selectHuman.SelectedIndex].ToString().Split('|');
                Misc._probsHuman = _pHuman[2] + " " + _pHuman[1];
                Misc._secondSubsPosition = _pSubs[2];
                Misc._secondSubsPFIO = _pSubs[1];
                this.Close();
            }
            
        }
    }
}
