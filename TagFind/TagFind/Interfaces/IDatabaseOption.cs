using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Interfaces
{
    internal interface IDatabaseOption
    {
        public void Add();
        public void Move();
        public void Remove();
        public void Rename();
        /// <summary>
        /// Update related data and modified time
        /// </summary>
        public void Update();
    }
}
