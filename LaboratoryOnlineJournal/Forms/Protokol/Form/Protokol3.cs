﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NPOI.HSSF.UserModel;
//using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Util;

namespace LaboratoryOnlineJournal
{
    public static partial class Misc
    {
        public static bool OtchProtokolType3(Protokols_class.SGroup_class.Protokol_class Protokol, bool CreateNew = true, bool Open = true)
        {
            {
                var MsgErr = "";

                for (int i = 0; i < Protokol.MarkCount; i++)
                {
                    var PAMIndex = -1;

                    for (int j = 0; j < Protokol.SampleCount; j++)
                    {
                        if (Protokol[j][i].LocalAlow && Protokol[j][i].Method.Length > 0)
                        {
                            if (PAMIndex < 0)
                            { PAMIndex = j; }
                            else if (Protokol[j][i].Method != Protokol[PAMIndex][i].Method)
                            {
                                MsgErr += '\n' + Protokol[j][i].Mark + " имеет различные методы у нормативов " + T.Object.Rows.Get<string>(Protokol[j].ObjectID, C.Object.Norm, C.Norm.Name) + " и " + T.Object.Rows.Get<string>(Protokol[PAMIndex].ObjectID, C.Object.Norm, C.Norm.Name);
                            }
                        }
                    }
                }

                if (MsgErr.Length > 0)
                {
                    MessageBox.Show(MsgErr, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            string NewFileName;
            {
                int Month, Year;

                ATMisc.GetYearMonthFromYM(Employe_Form.SPoints.YM, out Year, out Month);

                {
                    NewFileName = Application.StartupPath + "\\Отчеты\\";

                    if (!Directory.Exists(NewFileName)) Directory.CreateDirectory(NewFileName);

                    NewFileName += T.Podr.Rows.Get<string>(Protokol.PodrID, C.Podr.ShrName) + "\\";

                    if (!Directory.Exists(NewFileName)) Directory.CreateDirectory(NewFileName);

                    NewFileName += ATMisc.GetMonthName1(Month) + "\\";

                    if (!Directory.Exists(NewFileName)) Directory.CreateDirectory(NewFileName);
                }

                NewFileName += ProtokolFileName(Protokol);
            }

            if (CreateNew || !File.Exists(NewFileName))
            {
                var WorkBook = ATMisc.GetGenericExcel("Протокол испытаний тип 3.xls");

                if (WorkBook == null) return false;

                var TitleSheet = WorkBook.GetSheet("Заголовок");
                NPOI.SS.UserModel.ISheet Sheet1;

                if (TitleSheet == null)
                {
                    MessageBox.Show("В шаблоне не найден лист \"Заголовок\", вывод невозможен.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                var DT = Protokol.Date;

                GetProtokolsExchanges(TitleSheet
                                    , ATMisc.GetDateTimeFromYM(Employe_Form.SPoints.YM).Year
                                    , Protokol.Number.ToString() + "-РЗ-" + DT.Month.ToString() + "/" + DT.Year.ToString()
                                    , Protokol.Objects + ' ' + T.Podr.Rows.Get<string>(Protokol.PodrID, C.Podr.ShrName)
                                    , Protokol.Objects
                                    , T.PType.Rows.Get<string>(Protokol[0].PTypeID, C.PType.Name)
                                    , Protokol.ObjectsLocations
                                    , Protokol.DateOstr
                                    , Protokol.DateP
                                    , Protokol.StrTime
                                    , Protokol.Peoples
                                    , Protokol.Causes
                                    , Protokol.Numbers
                                    , DT.Day.ToString()
                                    , ATMisc.GetMonthName2(DT.Month)
                                    , DT.Month.ToString()
                                    , RCache.PSG.GetMethodName(Protokol.PodrID)
                                    , T.PaPoS.Rows.Get<string>(Protokol.PaPoSID, C.PaPoS.Name)
                                    , T.Podr.Rows.Get<string>(Protokol.PodrID, C.Podr.FllName)
                                    , T.Podr.Rows.Get<string>(Protokol.PodrID, C.Podr.ShrName)
                                    , T.Podr.Rows.Get<string>(Protokol.PodrID, C.Podr.Contact)
                                    , 0).Exchange();
                {
                    var Exchanges = new CellExchange_Class(TitleSheet);

                    Exchanges.ClearExchanges();

                    Exchanges.AddColumn("{имя свойства}");
                    Exchanges.AddColumn("{ед. свойства}");
                    Exchanges.AddColumn("{значение свойства}");
                }

                var Marks = new bool[RCache.Marks.Count];
                int TotalMarksCount = 0;

                var MsgErr = "";
                //чищу показатели
                for (int i = 0; i < RCache.Marks.Count; i++)
                {
                    for (int j = 0; j < Protokol.SampleCount; j++)
                    { Marks[i] = Marks[i] || Protokol[j][i].Alow; }

                    if (Marks[i])
                    {
                        int PIndex = -1;
                        for (int j = 0; j < Protokol.SampleCount; j++)
                        {
                            if (Protokol[j][i].Method.Length > 0)
                            {
                                PIndex = j;
                                goto FindAnotherOne;
                            }
                        }

                        Marks[i] = false;
                        continue;
                    FindAnotherOne: ;

                        var CanAdd = true;
                        for (int j = PIndex + 1; j < Protokol.SampleCount; j++)
                        {
                            CanAdd = true;
                            if (Protokol[j][i].Method.Length == 0 || Protokol[j][i].Method != Protokol[PIndex][i].Method)
                            {
                                MsgErr += '\n' + Protokol[j][i].Mark + " имеет различные методы у нормативов " + T.Object.Rows.Get<string>(Protokol[j].ObjectID, C.Object.Norm, C.Norm.Name) + " и " + T.Object.Rows.Get<string>(Protokol[PIndex].ObjectID, C.Object.Norm, C.Norm.Name);

                                Marks[i] = false;
                                goto TryNext;
                            }
                        }

                        Marks[i] = CanAdd;
                        if (CanAdd)
                        { TotalMarksCount++; }
                    TryNext: ;

                    }
                }

                if (MsgErr.Length > 0)
                {
                    MessageBox.Show("Ошибки:" + MsgErr, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (TotalMarksCount == 0)
                {
                    MessageBox.Show("Заполненые показатели не найдены.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                string UnDeleteSheetName;

                switch (Protokol.SGroup)
                {
                    case data.SGroup.Group3:

                        UnDeleteSheetName = "Изначальный";
                        {
                            int Index = WorkBook.GetSheetIndex(UnDeleteSheetName);
                            Sheet1 = WorkBook.GetSheetAt(Index);
                            WorkBook.SetSheetName(Index, "Концентрации");
                        }

                        if (Sheet1 == null)
                        {
                            MessageBox.Show("В шаблоне не найден лист \"" + UnDeleteSheetName + "\", вывод невозможен.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return false;
                        }

                        var SPointIndex = new SColumn_struct(-1, null);
                        var OEdTypeIndex = new SColumn_struct(-1, null);
                        var OMethodIndex = new SColumn_struct(-1, null);
                        var OMarkNameIndex = new SColumn_struct(-1, null);
                        var NumberIndex = new SColumn_struct(-1, null);
                        var ResultIndex = new SColumn_struct(-1, null);
                        var ProbeIndex = new SColumn_struct(-1, null);
                        var NormIndex = new SColumn_struct(-1, null);
                        int RowIndex = -1;

                        var SPStyle = new NPOI.SS.UserModel.ICellStyle[3];  //верх, середина, низ
                        var PStyle = new NPOI.SS.UserModel.ICellStyle[3];  //верх, середина, низ
                        {
                            var OEdTypeRowIndex = -1;
                            var OMethodRowIndex = -1;
                            var OMarkNameRowIndex = -1;
                            var NumberRowIndex = -1;
                            var SPointRowIndex = -1;
                            var ResultRowIndex = -1;
                            var NormRowIndex = -1;
                            var ProbeRowIndex = -1;

                            var ExistColumn = new CellExchange_Class(Sheet1);

                            ExistColumn.AddExchange("{номер п/п}", (Cell) =>
                            {
                                NumberRowIndex = Cell.RowIndex;
                                NumberIndex = new SColumn_struct(Cell);
                            }, 5);
                            ExistColumn.AddExchange("{место отбора}", (Cell) =>
                            {
                                SPointRowIndex = Cell.RowIndex;
                                SPointIndex = new SColumn_struct(Cell);

                                SPStyle[0] = Sheet1.Workbook.CreateCellStyle(); //верх
                                CopyStyleFromCell(SPStyle[0], Cell); SPStyle[0].BorderBottom = NPOI.SS.UserModel.BorderStyle.None;
                                SPStyle[1] = Sheet1.Workbook.CreateCellStyle(); //середина
                                CopyStyleFromCell(SPStyle[1], Cell); SPStyle[1].BorderBottom = SPStyle[1].BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                                SPStyle[2] = Sheet1.Workbook.CreateCellStyle(); //низ
                                CopyStyleFromCell(SPStyle[2], Cell); SPStyle[2].BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                            }, 5);
                            ExistColumn.AddExchange("{показатель}", (Cell) =>
                            {
                                OMarkNameRowIndex = Cell.RowIndex;
                                OMarkNameIndex = new SColumn_struct(Cell);
                            }, 5);
                            ExistColumn.AddExchange("{проба}", (Cell) =>
                            {
                                ProbeRowIndex = Cell.RowIndex;
                                ProbeIndex = new SColumn_struct(Cell);

                                PStyle[0] = Sheet1.Workbook.CreateCellStyle(); //верх
                                CopyStyleFromCell(PStyle[0], Cell); PStyle[0].BorderBottom = NPOI.SS.UserModel.BorderStyle.None;
                                PStyle[1] = Sheet1.Workbook.CreateCellStyle(); //середина
                                CopyStyleFromCell(PStyle[1], Cell); PStyle[1].BorderBottom = PStyle[1].BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                                PStyle[2] = Sheet1.Workbook.CreateCellStyle(); //низ
                                CopyStyleFromCell(PStyle[2], Cell); PStyle[2].BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                            }, 5);
                            ExistColumn.AddExchange("{ед.изм.}", (Cell) =>
                            {
                                OEdTypeRowIndex = Cell.RowIndex;
                                OEdTypeIndex = new SColumn_struct(Cell);
                            }, 5);
                            ExistColumn.AddExchange("{методика}", (Cell) =>
                            {
                                OMethodRowIndex = Cell.RowIndex;
                                OMethodIndex = new SColumn_struct(Cell);
                            }, 5);
                            ExistColumn.AddExchange("{результат}", (Cell) =>
                            {
                                ResultRowIndex = Cell.RowIndex;
                                ResultIndex = new SColumn_struct(Cell);
                            }, 5);
                            ExistColumn.AddExchange("{Номер протокола}", Protokol.Number.ToString() + "-РЗ-" + DT.Month.ToString() + "/" + DT.Year.ToString(), 5);
                            ExistColumn.AddExchange("{Дата}", DT.ToShortDateString(), 5);
                            ExistColumn.AddExchange("{норматив}", (Cell) =>
                            {
                                NormRowIndex = Cell.RowIndex;
                                NormIndex = new SColumn_struct(Cell);
                            }, 5);

                            SetResp(ExistColumn, Protokol.PodrID, data.TResp.LaboratoryProtokol);

                            if (SPointRowIndex == -1 || OMarkNameRowIndex == -1 || OEdTypeRowIndex == -1 || OMethodRowIndex == -1 || ResultRowIndex == -1)
                            {
                                MessageBox.Show("Не все табличные метки найдены.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return false;
                            }

                            if (NormRowIndex > -1 && NormRowIndex != SPointRowIndex || ProbeRowIndex > -1 && ProbeRowIndex != OMarkNameRowIndex || NumberRowIndex > -1 && NumberRowIndex != OMarkNameRowIndex || SPointRowIndex != OMarkNameRowIndex || OMarkNameRowIndex != OEdTypeRowIndex || OEdTypeRowIndex != OMethodRowIndex || OMethodRowIndex != ResultRowIndex)
                            {
                                MessageBox.Show("Все табличные метки должны распологаться в одной строке.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return false;
                            }

                            RowIndex = SPointRowIndex;
                        }

                        Sheet1.ShiftRows(RowIndex, Sheet1.LastRowNum, Protokol.SampleCount * TotalMarksCount - 1);

                        int ONumber = 0;

                        for (int i = 0; i < Protokol.TableCount; i++)
                        {
                            NPOI.SS.UserModel.IRow Row;

                            for (int j = 0; j < Protokol[i].MarkCount; j++)
                            {
                                Row = Sheet1.CreateRow(RowIndex++);

                                var Norm = RCache.Marks[Protokol.GetMarkID(j)].GetNorm(Protokol[i].NormID);

                                if (NumberIndex.Index > -1)
                                { ATMisc.SetValue(Row, ++ONumber, NumberIndex.Index, NumberIndex.Style); }

                                if (ProbeIndex.Index > -1)
                                { ATMisc.SetValue(Row, Protokol[i].Number, ProbeIndex.Index, ProbeIndex.Style); }   //тут у нас кол-во замеров равно кол-ву строк таблицы

                                if (NormIndex.Index > -1)
                                {
                                    switch (Norm.NType)
                                    {
                                        case data.NType.Mark:
                                            ATMisc.SetValue(Row, Norm.Range.Range, NormIndex.Index, NormIndex.Style);
                                            break;
                                        case data.NType.PodrV:
                                        case data.NType.PodrK:
                                        case data.NType.PodrAll:
                                            var PIndex = RCache.Marks.Norms.GetPodrIndex(Norm.NormID, T.SPoint.Rows.Get_UnShow<uint>(Protokol[i].SPointID, C.SPoint.Podr));
                                            ATMisc.SetValue(Row, Norm.Station(PIndex).Range, NormIndex.Index, NormIndex.Style);
                                            break;
                                        case data.NType.Volume:
                                            var VIndex = RCache.Marks.Norms.GetVolumeIndex(Norm.NormID, T.SPoint.Rows.Get_UnShow<uint>(Protokol[i].SPointID, C.SPoint.Object, C.Object.OLocationTo));
                                            ATMisc.SetValue(Row, Norm.Volume(VIndex).Range, NormIndex.Index, NormIndex.Style);
                                            break;
                                    }
                                }

                                ATMisc.SetValue(Row, Protokol.GetMethod(j), OMethodIndex.Index, OMethodIndex.Style);
                                ATMisc.SetValue(Row, Protokol.GetMarkName(j), OMarkNameIndex.Index, OMarkNameIndex.Style);
                                ATMisc.SetValue(Row, Protokol.GetMarkEdType(j), OEdTypeIndex.Index, OEdTypeIndex.Style);

                                if (Protokol.IsSpetialOut(j, i))
                                { ATMisc.SetValue(Row, Protokol.GetSpetialOut(j, i), ResultIndex.Index, ResultIndex.Style); }
                                else
                                { ATMisc.SetValue(Row, Protokol.GetMarkAmount(j, i), ResultIndex.Index, ResultIndex.Style); }
                            }

                            /*if (PAMIndex > -1)
                            {
                                Sheet1.AddMergedRegion(new CellRangeAddress(SameMethodRowIndex, RowIndex - 1, OMethodIndex.Index, OMethodIndex.Index));
                            }*/
                            Row = Sheet1.GetRow(RowIndex - TotalMarksCount);

                            switch (TotalMarksCount)
                            {
                                case 1:
                                    ATMisc.SetValue(Row, Protokol[i].SPointName, SPointIndex.Index, SPointIndex.Style);
                                    break;
                                case 2:
                                    Sheet1.AddMergedRegion(new CellRangeAddress(RowIndex - TotalMarksCount, RowIndex - 1, SPointIndex.Index, SPointIndex.Index));
                                    ATMisc.SetValue(Row, Protokol[i].SPointName, SPointIndex.Index, SPStyle[0]);
                                    Sheet1.GetRow(RowIndex - 1).CreateCell(SPointIndex.Index).CellStyle = SPStyle[2];

                                    if (ProbeIndex.Index > -1)
                                    {
                                        Sheet1.GetRow(RowIndex - 1).CreateCell(ProbeIndex.Index).CellStyle = PStyle[2];
                                        Sheet1.AddMergedRegion(new CellRangeAddress(RowIndex - TotalMarksCount, RowIndex - 1, ProbeIndex.Index, ProbeIndex.Index));
                                    }
                                    break;
                                default:
                                    Sheet1.AddMergedRegion(new CellRangeAddress(RowIndex - TotalMarksCount, RowIndex - 1, SPointIndex.Index, SPointIndex.Index));

                                    ATMisc.SetValue(Row, Protokol[i].SPointName, SPointIndex.Index, SPStyle[0]);

                                    if (ProbeIndex.Index > -1)
                                    {
                                        Sheet1.GetRow(RowIndex - 1).CreateCell(ProbeIndex.Index).CellStyle = PStyle[2];
                                        Sheet1.AddMergedRegion(new CellRangeAddress(RowIndex - TotalMarksCount, RowIndex - 1, ProbeIndex.Index, ProbeIndex.Index));

                                        for (int s = TotalMarksCount - 2; s > 1; s--)
                                        {
                                            Sheet1.GetRow(RowIndex - 1 - s).GetCell(ProbeIndex.Index).CellStyle = PStyle[1];
                                            Sheet1.GetRow(RowIndex - 1 - s).GetCell(SPointIndex.Index).CellStyle = SPStyle[1];
                                        }

                                        Sheet1.GetRow(RowIndex - 1).CreateCell(ProbeIndex.Index).CellStyle = PStyle[2];
                                        Sheet1.GetRow(RowIndex - 1).GetCell(SPointIndex.Index).CellStyle = SPStyle[2];
                                    }
                                    else
                                    {
                                        for (int s = TotalMarksCount - 2; s > 1; s--)
                                        { Sheet1.GetRow(RowIndex - 1 - s).GetCell(SPointIndex.Index).CellStyle = SPStyle[1]; }

                                        Sheet1.GetRow(RowIndex - 1).GetCell(SPointIndex.Index).CellStyle = SPStyle[2];
                                    }
                                    break;
                            }
                        }
                        break;
                    default: throw new Exception("Не верный тип протокола");
                }

                for (int i = 0; i < WorkBook.NumberOfSheets; i++)
                {
                    if (WorkBook.GetSheetAt(i).SheetName.ToLower() != "заголовок" && WorkBook.GetSheetAt(i).SheetName.ToLower() != "концентрации")
                    {
                        WorkBook.RemoveSheetAt(i);
                        i--;
                    }
                }

                return SaveExcel(WorkBook, NewFileName, Open);
            }
            else
            {
                if (Open)
                { System.Diagnostics.Process.Start(NewFileName); }

                return true;
            }
        }
    }
}