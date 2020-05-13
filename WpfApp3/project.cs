using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp3
{
    class Property
    {
        public string name { get; set; }
        public string value { get; set; }
        public Property(string Name, string Value)
        {
            name = Name;
            value = Value;
        }
    }
    class Asset
    {
        //Type is required as property to define node type in neo4j
        //Asset can be of type project, machine or componentScope
        public List<Property> propertyList { get; set; }
        //List to store characteristics (Specified_By relation)
        public List<Characteristic> characteristicList { get; set; }
        //List to store subassets (Decomposed_By relation)
        public List<Measure> measureList { get; set; }
        //List to store subassets (Decomposed_By relation)
        public List<Requirement> requirementList { get; set; }
        //List to store subassets (Decomposed_By relation)
        public List<Statement> statementList { get; set; }
        //List to store subassets (Decomposed_By relation)
        public List<Asset> subAssetList { get; set; }
    }
    class Characteristic
    {
        public string name { get; set; }
        public string description { get; set; }
        public string unit { get; set; }
        public string format { get; set; }
        public DateTime timestampCreate { get; set; }
        public int stakeholderIdCreate { get; set; }
        public string value { get; set; }
        public string prefix { get; set; }
    }
    //class Statement
    //{
    //    public List<Property> propertyList { get; set; }
    //}
    class Requirement
    {
        public List<Property> propertyList { get; set; }
    }
    class Measure
    {
        public List<Property> propertyList { get; set; }
    }
    class AssetMachine
    {
        public string name { get; set; }
        public string itemNR { get; set; }
        public string contractNR { get; set; }
        public List<Characteristic> characteristics { get; set; }
        public List<AssetScopeOfSupply> assetScopeOfSupply { get; set; }
    }

    class AssetScopeOfSupply
    { 
        public List<Characteristic> characteristics { get; set; }
    }

    class Statement
    {
        public string statement { get; set; }
        public DateTime timestampCreate { get; set; }
        public int stakeholderIdCreate { get; set; }
    }

    class AssetGroup
    {
        public string name { get; set; }
        public string itemNR { get; set; }
        public string contractNR { get; set; }
        public List<AssetGroup> assetGroups { get; set; }
        public List<Characteristic> characteristics { get; set; }
        public List<AssetMachine> assetMachines { get; set; }
    }

    class project
    {
        public AssetGroup projectFile;
        public AssetGroup loadProject()
        {
            AssetGroup ag = new AssetGroup();

            return ag;
        }

        public void readProjectFromExcel()
        {
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("C:/Users/32477/Downloads/213032 EQUIPMENT LIST_revF.xls");
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;

            string activeComponent = ""; //G, S, C, M
            //Create project
            projectFile = new AssetGroup();
            projectFile.assetGroups = new List<AssetGroup>();
            projectFile.assetMachines = new List<AssetMachine>();
            projectFile.characteristics = new List<Characteristic>();

            AssetGroup ag;
            AssetMachine am;
            AssetScopeOfSupply asos;
            Characteristic c;
            Statement t;


            int rowCount = xlRange.Rows.Count;
            int groupCount = 0;

            List<string[]> assets = new List<string[]>();

            int index = 0;

            for (int i = 1; i <= rowCount; i++)
            {
                if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
                {
                    //First column of row contains indication letter
                    //Store indication letter to activecomponent
                    activeComponent = xlRange.Cells[i, 1].Value2.ToString();
                    switch (activeComponent)
                    {
                        case "G":
                            if (groupCount == 0)
                            {
                                projectFile.itemNR = (xlRange.Cells[i, 3].Value2 ?? "").ToString();
                                projectFile.contractNR = (xlRange.Cells[i, 4].Value2 ?? "").ToString();
                                projectFile.name = (xlRange.Cells[i, 5].Value2 ?? "").ToString();
                            }
                            else
                            {
                                ag = new AssetGroup();
                                ag.name = (xlRange.Cells[i, 5].Value2 ?? "").ToString() + " " + (xlRange.Cells[i, 6].Value2 ?? "").ToString();
                                ag.itemNR = (xlRange.Cells[i, 3].Value2 ?? "").ToString();
                                ag.contractNR = (xlRange.Cells[i, 4].Value2 ?? "").ToString();
                                ag.characteristics = new List<Characteristic>();
                                ag.assetGroups = new List<AssetGroup>();
                                ag.assetMachines = new List<AssetMachine>();
                                string nr = (xlRange.Cells[i, 3].Value2 ?? "").ToString();
                                index = 4 - nr.Count(x => x == '0');
                                if (index == 4) index = 3;
                                switch (index)
                                {
                                    case 1:
                                        projectFile.assetGroups.Add(ag);
                                        break;
                                    case 2:
                                        projectFile.assetGroups.Last().assetGroups.Add(ag);
                                        break;
                                }
                            }
                            groupCount++;
                            break;
                        case "M":
                                                            string nr2 = (xlRange.Cells[i, 3].Value2 ?? "").ToString();
                                index = 4 - nr2.Count(x => x == '0');
                                if (index == 4) index = 3;
                            am = new AssetMachine();
                            am.itemNR = (xlRange.Cells[i, 3].Value2 ?? "").ToString();
                            am.contractNR = (xlRange.Cells[i, 4].Value2 ?? "").ToString();
                            am.name = (xlRange.Cells[i, 6].Value2 ?? "").ToString();
                            am.characteristics = new List<Characteristic>();
                            am.assetScopeOfSupply = new List<AssetScopeOfSupply>();
                            projectFile.assetGroups.Last().assetGroups.Last().assetMachines.Add(am);
                            break;
                        case "S":
                            break;
                        case "C":
                            c = new Characteristic();
                            c.name = (xlRange.Cells[i, 6].Value2 ?? "").ToString();
                            c.prefix = (xlRange.Cells[i, 8].Value2 ?? "").ToString();
                            c.value = (xlRange.Cells[i, 9].Value2 ?? "").ToString();
                            c.unit = (xlRange.Cells[i, 10].Value2 ?? "").ToString();
                            switch(index)
                            {
                                case 0:
                                    projectFile.characteristics.Add(c);
                                    break;
                                case 1:
                                    projectFile.assetGroups.Last().characteristics.Add(c);
                                    break;
                                case 2:
                                    projectFile.assetGroups.Last().assetGroups.Last().characteristics.Add(c);
                                    break;
                                case 3:
                                    projectFile.assetGroups.Last().assetGroups.Last().assetMachines.Last().characteristics.Add(c);
                                    break;
                            }
                            //Add Characteristic to parent group, machine or scope
                            break;
                        case "T":
                            t = new Statement();
                            t.statement = (xlRange.Cells[i, 6].Value2 ?? "").ToString();
                            //Add Statement to group, machine or scope
                            break;
                    }
                }
            }
        }
    }
}
