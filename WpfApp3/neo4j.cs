using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace WpfApp3
{
    class neo4j
    {
        private IDriver _driver;
        public void ConnectToDatabase()
        {
            //_driver = GraphDatabase.Driver("neo4j://e9ae164f.databases.neo4j.io", AuthTokens.Basic("neo4j", "hWbmhMUCaCGRVStvf1w_PBSLhD-ZNLWDXndG434U-Ws"), builder => builder.WithEncryptionLevel(EncryptionLevel.Encrypted));
            _driver = GraphDatabase.Driver("neo4j://localhost:7687");
        }
        //Add Node
        public int AddNode(string type, List<Property> lp)
        {
            using (var session = _driver.Session(o => o.WithDefaultAccessMode(AccessMode.Write)))
            {
                return session.WriteTransaction(tx => AddNodeB(tx, type, lp));
            }
        }
        private int AddNodeB(ITransaction tx, string type, List<Property> lp)
        {
            int c = 0;
            int projectId = -1;
            string query = "CREATE (a:" + type;
            query += "{";
            foreach (Property p in lp)
            {
                if (c > 0) query += ", ";
                c++;
                query += p.name + ": '" + p.value + "'";
            }
            query += "}) RETURN ID(a)";
            IResult res = tx.Run(query);
            foreach (var record in res)
            {
                projectId = Convert.ToInt32(record.Values["ID(a)"]);
            }
            return projectId;
        }
        //Add Relationship
        public int AddRelationshipNode(string type, List<Property> lp, int startNode, int endNode)
        {
            using (var session = _driver.Session(o => o.WithDefaultAccessMode(AccessMode.Write)))
            {
                return session.WriteTransaction(tx => AddRelationshipNodeB(tx, startNode, endNode, type, lp));
            }
        }
        private int AddRelationshipNodeB(ITransaction tx, int startNode, int endNode, string type, List<Property> lp)
        {
            string query = "MATCH (a),(b) WHERE ID(a) = " + startNode + " AND ID(b) = " + endNode + " CREATE (a)-[r:" + type + "]->(b)";
            tx.Run(query);
            return 0;
        }
        //Get all properties from Node
        public List<Property> GetAllProperties(int node)
        {
            using (var session = _driver.Session(o => o.WithDefaultAccessMode(AccessMode.Write)))
            {
                return session.WriteTransaction(tx => GetAllPropertiesB(tx, node));
            }
        }
        private List<Property> GetAllPropertiesB(ITransaction tx, int node)
        {
            List<Property> lp = new List<Property>();
            string query = "MATCH (a) WHERE ID(a) = " + node + " RETURN properties(a)";
            IResult res = tx.Run(query);
            var list = res.ToList();
            Dictionary<string,object> dict = (Dictionary<string,object>)list[0].Values["properties(a)"];
            foreach (KeyValuePair<string, object> item in dict)
            {
                lp.Add(new Property(item.Key, item.Value.ToString()));
            }
            return lp;
        }
        //Get all nodes with outgoing relationship from node
        public List<int> GetOutgoingNodes(int node, string relationshipType)
        {
            using (var session = _driver.Session(o => o.WithDefaultAccessMode(AccessMode.Write)))
            {
                return session.WriteTransaction(tx => GetOutgoingNodesB(tx, node, relationshipType));
            }
        }
        private List<int> GetOutgoingNodesB(ITransaction tx, int node, string relationshipType)
        {
            List<int> idList = new List<int>();
            string query = "match(n)-[r]-(a) WHERE id(n)=" + node + " AND type(r)='" + relationshipType + "' return id(a)";
            IResult res = tx.Run(query);
            foreach (var record in res)
            {
                idList.Add(Convert.ToInt32(record.Values["id(a)"]));
            }
            return idList;
        }
        //Get all nodes of machines
        public List<string[]> GetNodesFromType(string type)
        {
            using (var session = _driver.Session(o => o.WithDefaultAccessMode(AccessMode.Write)))
            {
                return session.WriteTransaction(tx => GetNodesFromTypeB(tx, type));
            }
        }
        private List<string[]> GetNodesFromTypeB(ITransaction tx, string type)
        {
            List<string[]> idList = new List<string[]>();
            string query = "match(a:" + type + ") return a.Name,a.itemNR,id(a)";
            IResult res = tx.Run(query);
            foreach (var record in res)
            {
                idList.Add(new string[3] { record.Values["a.itemNR"].ToString(), record.Values["id(a)"].ToString(), record.Values["a.Name"].ToString() });
            }
            return idList;
        }
        //Add data in neo4j 
        public bool insertAssetGroup(AssetGroup ag, int parentId)
        {
            
            List<Property> lp = new List<Property>();
            lp.Add(new Property("Name", ag.name));
            lp.Add(new Property("itemNR", ag.itemNR));
            lp.Add(new Property("contractNR", ag.contractNR));

            int assetId = AddNode("Group", lp);
            if (parentId >= 0)
            {
                AddRelationshipNode("Decomposed_By", lp, parentId, assetId);
            }
            //Add Characteristics
            foreach (Characteristic c in ag.characteristics)
            {
                lp = new List<Property>();
                lp.Add(new Property("Name", c.name ?? ""));
                lp.Add(new Property("Description", c.description ?? ""));
                lp.Add(new Property("Format", c.format ?? ""));
                lp.Add(new Property("Unit", c.unit ?? ""));
                lp.Add(new Property("Prefix", c.prefix ?? ""));
                lp.Add(new Property("Value", c.value ?? ""));
                lp.Add(new Property("stakeholderIdCreate", c.stakeholderIdCreate.ToString() ?? ""));
                lp.Add(new Property("stakeholderIdCreate", c.timestampCreate.ToString() ?? ""));

                int characteristicId = AddNode("Characteristic", lp);
                lp = new List<Property>();
                AddRelationshipNode("Specified_By", lp, assetId, characteristicId);

            }
            //Add Group
            foreach (AssetGroup a in ag.assetGroups)
            {
                insertAssetGroup(a, assetId);
            }
            //Add Group
            foreach (AssetMachine a in ag.assetMachines)
            {
                insertAssetMachine(a, assetId);
            }
            return true;
        }
        public bool insertAssetMachine(AssetMachine am, int parentId)
        {
            List<Property> lp = new List<Property>();
            lp.Add(new Property("Name", am.name));
            lp.Add(new Property("itemNR", am.itemNR));
            lp.Add(new Property("contractNR", am.contractNR));

            int assetId = AddNode("Machine", lp);
            if (parentId > 0)
            {
                AddRelationshipNode("Decomposed_By", lp, parentId, assetId);
            }
            //Add Characteristics
            foreach (Characteristic c in am.characteristics)
            {
                lp = new List<Property>();
                lp.Add(new Property("Name", c.name ?? ""));
                lp.Add(new Property("Description", c.description ?? ""));
                lp.Add(new Property("Format", c.format ?? ""));
                lp.Add(new Property("Unit", c.unit ?? ""));
                lp.Add(new Property("Prefix", c.prefix ?? ""));
                lp.Add(new Property("Value", c.value ?? ""));
                lp.Add(new Property("stakeholderIdCreate", c.stakeholderIdCreate.ToString() ?? ""));
                lp.Add(new Property("stakeholderIdCreate", c.timestampCreate.ToString() ?? ""));

                int characteristicId = AddNode("Characteristic", lp);
                lp = new List<Property>();
                AddRelationshipNode("Specified_By", lp, assetId, characteristicId);

            }
            return true;
        }
        //Load data from neo4j
        public List<List<Property>> getCharacteristics(int node)
        {
            //Get ids from nodes with characteristics of node
            List<int> nodeIds = GetOutgoingNodes(node, "Specified_By");
            //Get properties from node with id
            List<List<Property>> llp = new List<List<Property>>();
            foreach(int id in nodeIds)
            {
                llp.Add(GetAllProperties(id));
            }
            return llp;
        }
        //Get list with all machines and nodeId
        public List<string[]> GetListOfNodes(string type)
        {
            return GetNodesFromType(type);
        }


    }
}
