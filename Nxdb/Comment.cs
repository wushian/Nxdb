﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.basex.data;
using org.basex.query.item;

namespace Nxdb
{
    public class Comment : TreeNode
    {
        internal Comment(ANode aNode, Database database) : base(aNode, database, Data.COMM) { }
    }
}
