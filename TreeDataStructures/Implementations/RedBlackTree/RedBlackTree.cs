using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    private bool IsBlack(RbNode<TKey, TValue>? node) => node == null || node.Color == RbColor.Black;

    private bool IsRed(RbNode<TKey, TValue>? node) => !IsBlack(node);

    private void SetBlack(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Black;
    }

    private void SetRed(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Red;
    }

    private void SetColorFrom(RbNode<TKey, TValue>? source, RbNode<TKey, TValue>? target)
    {
        if (source != null && target != null)
        {
            target.Color = source.Color;
        }
    }

    private RbNode<TKey, TValue>? GetSibling(RbNode<TKey, TValue> parent, bool nodeIsLeftChild) => nodeIsLeftChild ? parent.Right : parent.Left;

    private RbNode<TKey, TValue>? GetNearNephew(RbNode<TKey, TValue>? subling, bool nodeIsLeftChild) => nodeIsLeftChild ? subling?.Left : subling?.Right;

    private RbNode<TKey, TValue>? GetFarNephew(RbNode<TKey, TValue>? subling, bool nodeIsLeftChild) => nodeIsLeftChild ? subling?.Right : subling?.Left;

    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);

    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        var node = newNode;
        while (node != null)
        {
            if (node.Parent == null)
            {
                AddCase1(node);
                break;
            }
            else if (IsBlack(node.Parent))
            {
                AddCase2(node);
                break;
            }
            else if (IsRed(node.Uncle))  // Parent is Red;
            {
                AddCase3(node);
                node = node.Grandparent;
            }
            else  // Parent is Red, Uncle is Black
            {
                AddCases45(node);
                break;
            }
        }
        if (IsRed(this.Root)) SetBlack(this.Root);
    }

    private void AddCase1(RbNode<TKey, TValue> node)
    {
        SetBlack(node);
    }

    private void AddCase2(RbNode<TKey, TValue> node) { }

    private void AddCase3(RbNode<TKey, TValue> node)
    {
        SetBlack(node.Parent);
        SetBlack(node.Uncle);
        SetRed(node.Grandparent);
    }

    private void AddCases45(RbNode<TKey, TValue> node)
    {
        if (node.Parent!.IsLeftChild)
        {
            if (node.IsRightChild)
            {
                RotateLeft(node.Parent);
                RotateRight(node.Parent);
                SetBlack(node);
                SetRed(node.Right);
            }
            else
            {
                RotateRight(node.Grandparent!);
                SetBlack(node.Parent);
                SetRed(node.Parent.Right);
            }
        }
        else
        {
            if (node.IsLeftChild)
            {
                RotateRight(node.Parent);
                RotateLeft(node.Parent);
                SetBlack(node);
                SetRed(node.Left);
            }
            else
            {
                RotateLeft(node.Grandparent!);
                SetBlack(node.Parent);
                SetRed(node.Parent.Left);
            }
        }
    }

    private RbColor _lastDeleteNodeColor;

    protected override void RemoveNode(RbNode<TKey, TValue> node) {
        RbNode<TKey, TValue> deleteNode = node;
        RbNode<TKey, TValue>? parent = deleteNode.Parent;
        RbNode<TKey, TValue>? replacement;

        if (deleteNode.Left != null && deleteNode.Right != null) {
            deleteNode = FindMinimum(deleteNode.Right)!;
            parent = deleteNode.Parent;
            node.Key = deleteNode.Key;
            node.Value = deleteNode.Value;
        }

        replacement = deleteNode.Left ?? deleteNode.Right;
        _lastDeleteNodeColor = deleteNode.Color;
        Transplant(deleteNode, replacement);
        OnNodeRemoved(parent, replacement);
    }

    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (_lastDeleteNodeColor == RbColor.Red)
        {
            RemoveCase1();
            return;
        }
        
        if (_lastDeleteNodeColor == RbColor.Black && IsRed(child))
        {
            RemoveCase2(child);
            return;
        }

        RbNode<TKey, TValue>? node = child;
        bool nodeIsLeftChild;
        RbNode<TKey, TValue>? sibling, nearNephew, farNephew;
        while (node != this.Root && IsBlack(node))
        {
            nodeIsLeftChild = node == parent.Left;
            sibling = GetSibling(parent, nodeIsLeftChild);
            nearNephew = GetNearNephew(sibling, nodeIsLeftChild);
            farNephew = GetFarNephew(sibling, nodeIsLeftChild);
            if (IsRed(sibling))
            {
                RemoveCase3(parent, sibling, nodeIsLeftChild);
                continue;
            }
            if (IsBlack(nearNephew) && IsBlack(farNephew))  // siblind is black
            {
                RemoveCase4(parent, sibling);
                if (IsRed(parent))
                {
                    SetBlack(parent);
                    break;
                }
                node = parent;
                parent = node.Parent;
                continue;
            }
            if (IsRed(nearNephew) && IsBlack(farNephew))
            {
                RemoveCase5(sibling, nearNephew, nodeIsLeftChild);
                continue;
            }
            if (IsRed(farNephew))
            {
                RemoveCase6(parent, sibling, farNephew, nodeIsLeftChild);
                break;
            }
        }
        if (IsRed(this.Root)) { SetBlack(this.Root); }
    }

    private void RemoveCase1() { }

    private void RemoveCase2(RbNode<TKey, TValue>? node) { SetBlack(node); }

    private void RemoveCase3(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? sibling, bool nodeIsLeftChild)
    {
        SetBlack(sibling);
        SetRed(parent);
        if (nodeIsLeftChild)
        { 
            RotateLeft(parent);
        }
        else
        {
            RotateRight(parent);
        }
    }

    private void RemoveCase4(RbNode<TKey, TValue> parent, RbNode<TKey, TValue>? sibling)
    {
        SetRed(sibling);
    }

    private void RemoveCase5(RbNode<TKey, TValue>? sibling, RbNode<TKey, TValue>? nearNephew, bool nodeIsLeftChild)
    {
        SetBlack(nearNephew);
        SetRed(sibling);
        if (nodeIsLeftChild)
        {
            RotateRight(sibling);
        }
        else
        {
            RotateLeft(sibling);
        }
    }

    private void RemoveCase6(RbNode<TKey, TValue> parent, RbNode<TKey, TValue>? sibling, RbNode<TKey, TValue>? farNephew, bool nodeIsLeftChild)
    {
        SetBlack(farNephew);
        SetColorFrom(parent, sibling);
        SetBlack(parent);
        if (nodeIsLeftChild)
        {
            RotateLeft(parent);
        }
        else
        {
            RotateRight(parent);
        }
    }
}