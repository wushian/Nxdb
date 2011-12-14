﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.basex.data;
using org.basex.query.item;

namespace Nxdb
{
    public class ProcessingInstruction : TreeNode
    {
        internal ProcessingInstruction(ANode aNode, Database database) : base(aNode, database, Data.PI) { }
    }
}
