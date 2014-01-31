using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.Drawing {
    class UndoRedoActionsList {
        internal UndoRedoAction currentUndo;
        internal UndoRedoAction currentRedo;

        internal UndoRedoAction AddAction(UndoRedoAction action) {
            if (currentUndo != null)
                currentUndo.Next = action;

            action.Previous = currentUndo;
            currentUndo = action;
            currentRedo = null;

            return action;
        }
    }
}
