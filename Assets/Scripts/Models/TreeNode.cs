﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TreeNode<T> : IEnumerable<TreeNode<T>>
{

    public T Data { get; set; }
    public TreeNode<T> Parent { get; set; }
    public ICollection<TreeNode<T>> Children { get; set; }

    public bool IsRoot
    {
        get { return Parent == null; }
    }

    public bool IsLeaf
    {
        get { return Children.Count == 0; }
    }

    public int Level
    {
        get
        {
            if (this.IsRoot)
                return 0;
            return Parent.Level + 1;
        }
    }


    public TreeNode(T data)
    {
        this.Data = data;
        this.Children = new LinkedList<TreeNode<T>>();

        this.ElementsIndex = new LinkedList<TreeNode<T>>();
        this.ElementsIndex.Add(this);
    }

    public TreeNode<T> AddChild(T child)
    {
        TreeNode<T> childNode = new TreeNode<T>(child) { Parent = this };
        this.Children.Add(childNode);

        this.RegisterChildForSearch(childNode);

        return childNode;
    }

    public void AddChildren(List<T> children)
    {
        foreach (T child in children)
        {
            this.AddChild(child);
        }
    }

    public override string ToString()
    {
       return this.Data != null ? Data.ToString() : "[null]";
    }

    public string TreeToString()
    {
        string str = Parent == null ? "[ROOT]" : Data.ToString();
        str += Environment.NewLine;
        foreach (TreeNode<T> child in Children)
        {
            str += "".PadLeft(this.Level, ' ') + child.TreeToString();
        }
        return str;
    }


    #region searching

    private ICollection<TreeNode<T>> ElementsIndex { get; set; }

    private void RegisterChildForSearch(TreeNode<T> node)
    {
        ElementsIndex.Add(node);
        if (Parent != null)
            Parent.RegisterChildForSearch(node);
    }

    public TreeNode<T> FindTreeNode(Func<TreeNode<T>, bool> predicate)
    {
        return this.ElementsIndex.FirstOrDefault(predicate);
    }

    #endregion


    #region iterating

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TreeNode<T>> GetEnumerator()
    {
        yield return this;
        foreach (var directChild in this.Children)
        {
            foreach (var anyChild in directChild)
                yield return anyChild;
        }
    }

    #endregion
}