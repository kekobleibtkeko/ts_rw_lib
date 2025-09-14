using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using TS_Lib.Windows;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TS_Lib.Util;

public interface ITreeParent<TItem, TCategory>
{
	List<TreeNodeBase<TItem, TCategory>> Children { get; }
}

public interface ITreeCategory<TItem, TCategory>
{
	TCategory Category { get; }
	bool IsOpen { get; }
}

public interface ITreeItem<TItem, TCategory>
{
	TItem Item { get; }
}

public class Tree<TItem, TCategory>(
	Tree<TItem, TCategory>.TreeCategoryDrawDelegate categoryDrawFunc,
	Tree<TItem, TCategory>.TreeItemDrawDelegate itemDrawFunc
) : ITreeParent<TItem, TCategory>
{
	public delegate bool TreeItemDrawDelegate(ITreeItem<TItem, TCategory> item, Rect rect);
	public delegate bool TreeCategoryDrawDelegate(ITreeCategory<TItem, TCategory> category, Rect rect);

	public List<TreeNodeBase<TItem, TCategory>> UpperNodes = [];
	public int Indent;
	public float CategorySize = 20;
	public float ItemSize = 20;
	public bool Editable = true;

	public TreeCategoryDrawDelegate CategoryDrawFunc = categoryDrawFunc;
	public TreeItemDrawDelegate ItemDrawFunc = itemDrawFunc;
	public Action<Tree<TItem, TCategory>, ITreeParent<TItem, TCategory>>? AddRequestFunc;

	public List<TreeNodeBase<TItem, TCategory>> Children => UpperNodes;

	public float Measure()
	{
		return UpperNodes.Sum(x => x.Measure(this));
	}

	public bool Draw(Listing_Standard listing, TSUtil.ScrollPosition? scrollPosition = null)
	{
		bool changed = false;
		foreach (var node in UpperNodes)
		{
			changed = node.Draw(this, this, listing, scrollPosition) || changed;
		}
		if (AddRequestFunc is not null && listing.Row(30).ButtonIcon(TexButton.Add))
		{
			NotifyAddRequest(this);
		}
		return ChangeNotify<ITreeParent<TItem, TCategory>>.TryConsume(this) || changed;
	}

	public void NotifyAddRequest(ITreeParent<TItem, TCategory> requester) => AddRequestFunc?.Invoke(this, requester);
}

public abstract class TreeNodeBase<TItem, TCategory>(Tree<TItem, TCategory> tree)
{
	public Tree<TItem, TCategory> Tree => tree;
	public abstract bool Draw(Tree<TItem, TCategory> tree, ITreeParent<TItem, TCategory> parent, Listing_Standard listing, TSUtil.ScrollPosition? scrollPosition = null);
	public abstract float Measure(Tree<TItem, TCategory> tree);
}

public class TreeCategory<TItem, TCategory>(
	Tree<TItem, TCategory> tree,
	TCategory category,
	IEnumerable<TreeNodeBase<TItem, TCategory>>? children = null
) : TreeNodeBase<TItem, TCategory>(tree), ITreeParent<TItem, TCategory>, ITreeCategory<TItem, TCategory>
{
	public enum ContextType
	{
		Delete,
		Create,
	}

	public bool Open = true;
	public TCategory Category => category;
	public List<TreeNodeBase<TItem, TCategory>> Contained = [.. children ?? []];

	public List<TreeNodeBase<TItem, TCategory>> Children => Contained;

	public bool IsOpen => Open;

	public override bool Draw(Tree<TItem, TCategory> tree, ITreeParent<TItem, TCategory> parent, Listing_Standard listing, TSUtil.ScrollPosition? scrollPosition = null)
	{
		var rect = listing.GetRect(tree.CategorySize);
		if (Widgets.ButtonInvisible(rect))
		{
			if (TSUtil.IsRightClick)
			{
				TSUtil.Menu(
					TSUtil.GetEnumValues<ContextType>(),
					en =>
					{
						switch (en)
						{
							case ContextType.Delete:
								parent.RemoveNode(this);
								break;
							case ContextType.Create:
								Tree.NotifyAddRequest(this);
								break;
						}
					},
					en => $"TS.{typeof(ContextType).Name}.{en}".Translate(),
					enabled_func: en => en switch
					{
						ContextType.Create => Tree.AddRequestFunc is not null,
						ContextType.Delete or _ => true,
					}
				);
			}
			else
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				Open = !Open;
			}
		}
		var changed = false;
		changed = tree.CategoryDrawFunc(this, rect) || changed;
		if (Open)
		{
			listing.Indent();
			foreach (var child in Children)
			{
				changed = child.Draw(tree, this, listing, scrollPosition) || changed;
			}
			listing.Outdent();
		}
		return ChangeNotify<ITreeParent<TItem, TCategory>>.TryConsume(this) || changed;
	}

	public override float Measure(Tree<TItem, TCategory> tree)
	{
		float res = tree.CategorySize;
		if (Open)
			res += Contained.Sum(x => x.Measure(tree));
		return res;
	}
}

public class TreeItem<TItem, TCategory>(
	Tree<TItem, TCategory> tree,
	TItem item
) : TreeNodeBase<TItem, TCategory>(tree), ITreeItem<TItem, TCategory>
{
	public TItem Item => item;

	public override bool Draw(Tree<TItem, TCategory> tree, ITreeParent<TItem, TCategory> parent, Listing_Standard listing, TSUtil.ScrollPosition? scrollPosition = null)
	{
		var rect = listing.GetRect(tree.ItemSize);
		return tree.ItemDrawFunc(this, rect);
	}

	public override float Measure(Tree<TItem, TCategory> tree) => tree.ItemSize;
}


public static partial class TSUtil
{
	public readonly struct TreePath(string path)
	{
		public string RawPath { get; } = path;
		public string[] Segments { get; } = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

		public readonly string Root => Segments.First();
		public readonly string Name => Segments.Last();
		public readonly TreePath? Parent => Segments.Length > 1
			? new(string.Join("/", Segments.Take(Segments.Length - 1)))
			: null
		;

		public IEnumerator<string> SegmentEnumerator => Segments.AsEnumerable().GetEnumerator();
	}

	public static void AddNode<TItem, TCategory>(this ITreeParent<TItem, TCategory> parent, TreeNodeBase<TItem, TCategory> node)
	{
		parent.Children.Add(node);
		ChangeNotify<ITreeParent<TItem, TCategory>>.Notify(parent);
	}

	public static void RemoveNode<TItem, TCategory>(this ITreeParent<TItem, TCategory> parent, TreeNodeBase<TItem, TCategory> node)
	{
		if (parent.Children.Remove(node))
			ChangeNotify<ITreeParent<TItem, TCategory>>.Notify(parent);
	}

	public static Tree<T, string> BuildTree<T>(
		this IDictionary<string, T> dict,
		Tree<T, string>.TreeItemDrawDelegate itemDrawFunc
	)
	{
		var nodes_at_path = new Dictionary<TreePath, List<T>>();
		var paths_from = new Dictionary<TreePath, TreePath>();
		foreach (var (path_str, item) in dict)
		{
			var path = new TreePath(path_str);
			var list = nodes_at_path.Ensure(path);
			list.Add(item);

			TreePath last = path;
			TreePath? current = path.Parent;
			while (current.HasValue)
			{
				paths_from[current.Value] = last;
				current = current.Value.Parent;
			}
		}

		var cat_paths = new List<TreePath>();
		var tree = new Tree<T, string>(
			(cat, rect) =>
			{
				var icon_rect = rect.LeftPartPixels(rect.height);
				(
					cat.IsOpen
						? TexButton.Collapse
						: TexButton.Reveal
				).DrawFitted(icon_rect);
				using (new TextAnchor_D(TextAnchor.MiddleLeft))
					Widgets.Label(rect.ShrinkLeft(rect.height + 3), $"{new TreePath(cat.Category).Name}");
				return false;
			},
			itemDrawFunc
		);

		var path_cats = new Dictionary<TreePath, TreeCategory<T, string>>();
		foreach (var (to, from) in paths_from.OrderBy(kv => kv.Value.RawPath.Length))
		{
			var from_cat = path_cats.Ensure(from, () => new(tree, from.RawPath));

			TreeNodeBase<T, string> to_node;
			if (!nodes_at_path.TryGetValue(to, out var vals)
				|| vals.Count > 1
			)
			{
				to_node = path_cats.Ensure(to, () => new(tree, to.RawPath));
			}
			else
			{
				to_node = new TreeItem<T, string>(tree, vals.First());
			}

			from_cat.Contained.Add(to_node);
			if (from.Segments.Length == 1 && !tree.UpperNodes.Contains(from_cat))
			{
				tree.UpperNodes.Add(from_cat);
			}
		}

		tree.AddRequestFunc = (_, requester) =>
		{
			Find.WindowStack.Add(new Window_Input<string>(
				string.Empty,
				cat =>
				{
					if (string.IsNullOrEmpty(cat))
						requester.AddNode(new TreeCategory<T, string>(tree, "empty"));
					else
						requester.AddNode(new TreeCategory<T, string>(tree, cat));
				}
				)
			);
		};

		return tree;
	}

	public static void TreeFromPaths<TItem>(
		this Rect rect,
		IDictionary<string, TItem> dict
	)
	{

	}
}