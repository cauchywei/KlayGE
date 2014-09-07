﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Ribbon;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Windows.Forms;

namespace MtlEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : RibbonWindow
	{
		const int MESHES_ORDER = 0;
		const int AMBIENT_ORDER = 1;
		const int DIFFUSE_ORDER = 2;
		const int SPECULAR_ORDER = 3;
		const int SHININESS_ORDER = 4;
		const int EMIT_ORDER = 5;
		const int OPACITY_ORDER = 6;
		const int DIFFUSE_TEX_ORDER = 7;
		const int SPECULAR_TEX_ORDER = 8;
		const int SHININESS_TEX_ORDER = 9;
		const int BUMP_TEX_ORDER = 10;
		const int HEIGHT_TEX_ORDER = 11;
		const int EMIT_TEX_ORDER = 12;
		const int OPACITY_TEX_ORDER = 13;

		[CategoryOrder("Meshes", 0)]
		[CategoryOrder("Material", 1)]
		[CategoryOrder("Textures", 2)]
		public class ModelPropertyTypes
		{
			[Category("Meshes")]
			[DisplayName("Meshes")]
			[ItemsSource(typeof(MeshItemsSource))]
			[PropertyOrder(MESHES_ORDER)]
			public string meshes { get; set; }

			[Category("Material")]
			[DisplayName("Ambient")]
			[PropertyOrder(AMBIENT_ORDER)]
			public Color ambient { get; set; }
			[Category("Material")]
			[DisplayName("Diffuse")]
			[PropertyOrder(DIFFUSE_ORDER)]
			public Color diffuse { get; set; }
			[Category("Material")]
			[DisplayName("Specular")]
			[PropertyOrder(SPECULAR_ORDER)]
			public Color specular { get; set; }
			[Category("Material")]
			[DisplayName("Shininess")]
			[PropertyOrder(SHININESS_ORDER)]
			public float shininess { get; set; }
			[Category("Material")]
			[DisplayName("Emit")]
			[PropertyOrder(EMIT_ORDER)]
			public Color emit { get; set; }
			[Category("Material")]
			[DisplayName("Opacity")]
			[PropertyOrder(OPACITY_ORDER)]
			[Editor(typeof(SliderUserControlEditor), typeof(SliderUserControlEditor))]
			public float opacity { get; set; }

			[Category("Textures")]
			[DisplayName("Diffuse")]
			[PropertyOrder(DIFFUSE_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string diffuse_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Specular")]
			[PropertyOrder(SPECULAR_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string specular_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Shininess")]
			[PropertyOrder(SHININESS_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string shininess_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Bump")]
			[PropertyOrder(BUMP_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string bump_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Height")]
			[PropertyOrder(HEIGHT_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string height_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Emit")]
			[PropertyOrder(EMIT_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string emit_tex { get; set; }
			[Category("Textures")]
			[DisplayName("Opacity")]
			[PropertyOrder(OPACITY_TEX_ORDER)]
			[Editor(typeof(OpenTexUserControlEditor), typeof(OpenTexUserControlEditor))]
			public string opacity_tex { get; set; }
		}

		public MainWindow()
		{
			InitializeComponent();

			MeshItemsSource.items = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();

			properties_obj_ = new ModelPropertyTypes();
			properties.SelectedObject = properties_obj_;

			skinning.IsEnabled = false;
			play.IsEnabled = false;
			visualize.IsEnabled = false;
			frame_text.IsEnabled = false;
			frame_slider.IsEnabled = false;

			frame_slider.Minimum = 0;
			frame_slider.Maximum = 1;

			properties.IsEnabled = false;

			last_time_ = DateTime.Now;

			selected_mesh_index_ = 0;

			Uri iconUri = new Uri("pack://application:,,,/Images/klayge_logo.ico", UriKind.RelativeOrAbsolute);
			this.Icon = BitmapFrame.Create(iconUri);
		}

		void MainWindowLoaded(object sender, RoutedEventArgs e)
		{
			IntPtr wnd = editor_wnd.Handle;
			core_ = new MtlEditorCore(wnd);

			CompositionTarget.Rendering += this.MainWindowIdle;
		}
		void MainWindowUnloaded(object sender, RoutedEventArgs e)
		{
			CompositionTarget.Rendering -= this.MainWindowIdle;
			core_.Destroy();
		}

		private void MainWindowIdle(object sender, EventArgs e)
		{
			core_.Refresh();

			if (true == play.IsChecked)
			{
				DateTime this_time = DateTime.Now;
				if (this_time.Subtract(last_time_).TotalSeconds > 0.02)
				{
					frame_ += 0.02 * core_.ModelFrameRate();
					frame_ = frame_ % (float)core_.NumFrames();

					last_time_ = this_time;
				}

				frame_slider.Value = (int)(frame_ * 10 + 0.5f);
			}
		}
		private void EditorWindowSizeChanged(object sender, SizeChangedEventArgs e)
		{
			editor_frame.Width = editor_bg.ActualWidth;
			editor_frame.Height = editor_bg.ActualHeight;

			core_.Resize((uint)editor_frame.Width, (uint)editor_frame.Height);
		}

		private void OpenClick(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			dlg.DefaultExt = ".meshml";
			dlg.Filter = "All Model Files (*.meshml, *.model_bin)|*.meshml;*.model_bin|MeshML Files (*.meshml)|*.meshml|model_bin Files (*.model_bin)|*.model_bin|All Files|*.*";
			dlg.CheckPathExists = true;
			dlg.CheckFileExists = true;
			if (true == dlg.ShowDialog())
			{
				core_.OpenModel(dlg.FileName);

				if (core_.NumFrames() != 0)
				{
					skinning.IsEnabled = true;
					skinning.IsChecked = true;
					play.IsEnabled = true;
					frame_text.IsEnabled = true;
					frame_slider.IsEnabled = true;
					frame_slider.Maximum = core_.NumFrames() * 10 - 1;
				}
				else
				{
					skinning.IsEnabled = false;
					skinning.IsChecked = false;
					play.IsEnabled = false;
					frame_text.IsEnabled = false;
					frame_slider.IsEnabled = false;
					frame_slider.Maximum = 1;
				}
				visualize.IsEnabled = true;
				properties.IsEnabled = true;

				frame_ = 0;

				properties.SelectedObject = null;

				MeshItemsSource.items.Clear();
				MeshItemsSource.items.Add("");
				uint num_meshes = core_.NumMeshes();
				for (uint i = 0; i < num_meshes; ++ i)
				{
					MeshItemsSource.items.Add(core_.MeshName(i));
				}

				properties_obj_.meshes = "";
				this.UpdateMeshProperties(0);

				properties.SelectedObject = properties_obj_;
			}
		}

		private void SaveClick(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

			dlg.DefaultExt = ".meshml";
			dlg.Filter = "MeshML Files (*.meshml)|*.meshml|All Files|*.*";
			dlg.OverwritePrompt = true;
			if (true == dlg.ShowDialog())
			{
				core_.SaveModel(dlg.FileName);
			}
		}

		private void SkinningChecked(object sender, RoutedEventArgs e)
		{
			core_.SkinningOn(1);
			play.IsEnabled = true;
		}
		private void SkinningUnchecked(object sender, RoutedEventArgs e)
		{
			core_.SkinningOn(0);
			play.IsEnabled = false;
		}

		private void FPSCameraChecked(object sender, RoutedEventArgs e)
		{
			core_.FPSCameraOn(1);
		}
		private void FPSCameraUnchecked(object sender, RoutedEventArgs e)
		{
			core_.FPSCameraOn(0);
		}

		private void FrameSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			frame_ = frame_slider.Value * 0.1;
			core_.CurrFrame((float)frame_);
			frame_text.Content = "Frame " + (int)(frame_ + 0.5f);
		}

		private void VisualizeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (core_ != null)
			{
				System.Windows.Controls.Ribbon.RibbonGalleryItem item = e.NewValue as System.Windows.Controls.Ribbon.RibbonGalleryItem;
				core_.Visualize(Int32.Parse((string)item.DataContext));
			}
		}

		private void EditorMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			uint buttons = 0;
			if (MouseButtons.Left == e.Button)
			{
				buttons |= 1;
			}
			if (MouseButtons.Right == e.Button)
			{
				buttons |= 2;
			}
			if (MouseButtons.Middle == e.Button)
			{
				buttons |= 4;
			}
			core_.MouseDown(e.X, e.Y, buttons);
		}
		private void EditorMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			uint buttons = 0;
			if (MouseButtons.Left == e.Button)
			{
				buttons |= 1;
			}
			if (MouseButtons.Right == e.Button)
			{
				buttons |= 2;
			}
			if (MouseButtons.Middle == e.Button)
			{
				buttons |= 4;
			}
			core_.MouseUp(e.X, e.Y, buttons);

			if (MouseButtons.Left == e.Button)
			{
				uint selected_mesh = core_.SelectedMesh();
				if (selected_mesh != selected_mesh_index_)
				{
					this.UpdateMeshProperties(selected_mesh);
				}
			}
		}
		private void EditorMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			uint buttons = 0;
			if (MouseButtons.Left == e.Button)
			{
				buttons |= 1;
			}
			if (MouseButtons.Right == e.Button)
			{
				buttons |= 2;
			}
			if (MouseButtons.Middle == e.Button)
			{
				buttons |= 4;
			}
			core_.MouseMove(e.X, e.Y, buttons);
		}
		private void EditorKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			core_.KeyPress(e.KeyChar);
		}

		private void UpdateMeshProperties(uint mesh_index)
		{
			selected_mesh_index_ = mesh_index;
			core_.SelectMesh(mesh_index);

			properties.SelectedObject = null;

			properties_obj_.meshes = MeshItemsSource.items[(int)mesh_index].DisplayName;

			if (mesh_index > 0)
			{
				uint mat_id = core_.MaterialID(mesh_index - 1);

				properties_obj_.ambient = core_.AmbientMaterial(mat_id);
				properties_obj_.diffuse = core_.DiffuseMaterial(mat_id);
				properties_obj_.specular = core_.SpecularMaterial(mat_id);
				properties_obj_.emit = core_.EmitMaterial(mat_id);
				properties_obj_.shininess = core_.ShininessMaterial(mat_id);
				properties_obj_.opacity = core_.OpacityMaterial(mat_id);

				properties_obj_.diffuse_tex = core_.DiffuseTexture(mat_id);
				properties_obj_.specular_tex = core_.SpecularTexture(mat_id);
				properties_obj_.shininess_tex = core_.ShininessTexture(mat_id);
				properties_obj_.bump_tex = core_.BumpTexture(mat_id);
				properties_obj_.height_tex = core_.HeightTexture(mat_id);
				properties_obj_.emit_tex = core_.EmitTexture(mat_id);
				properties_obj_.opacity_tex = core_.OpacityTexture(mat_id);
			}
			else
			{
				properties_obj_.ambient = Color.FromArgb(0, 0, 0, 0);
				properties_obj_.diffuse = Color.FromArgb(0, 0, 0, 0);
				properties_obj_.specular = Color.FromArgb(0, 0, 0, 0);
				properties_obj_.emit = Color.FromArgb(0, 0, 0, 0);
				properties_obj_.shininess = 0;
				properties_obj_.opacity = 0;

				properties_obj_.diffuse_tex = "";
				properties_obj_.specular_tex = "";
				properties_obj_.shininess_tex = "";
				properties_obj_.bump_tex = "";
				properties_obj_.height_tex = "";
				properties_obj_.emit_tex = "";
				properties_obj_.opacity_tex = "";
			}

			properties.SelectedObject = properties_obj_;
		}

		private void PropertyGridValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
		{
			Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem item = e.OriginalSource as Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem;
			switch (item.PropertyOrder)
			{
			case MESHES_ORDER:
				{
					uint mesh_index = 0;
					for (; mesh_index < MeshItemsSource.items.Count; ++ mesh_index)
					{
						if (MeshItemsSource.items[(int)mesh_index].DisplayName == (e.NewValue as string))
						{
							break;
						}
					}

					this.UpdateMeshProperties(mesh_index);
				}
				break;

			case AMBIENT_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.AmbientMaterial(mat_id, properties_obj_.ambient);
				}
				break;

			case DIFFUSE_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.DiffuseMaterial(mat_id, properties_obj_.diffuse);
				}
				break;

			case SPECULAR_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.SpecularMaterial(mat_id, properties_obj_.specular);
				}
				break;

			case SHININESS_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.ShininessMaterial(mat_id, properties_obj_.shininess);
				}
				break;

			case EMIT_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.EmitMaterial(mat_id, properties_obj_.emit);
				}
				break;

			case OPACITY_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.OpacityMaterial(mat_id, properties_obj_.opacity);
				}
				break;

			case DIFFUSE_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.DiffuseTexture(mat_id, properties_obj_.diffuse_tex);
				}
				break;

			case SPECULAR_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.SpecularTexture(mat_id, properties_obj_.specular_tex);
				}
				break;

			case SHININESS_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.ShininessTexture(mat_id, properties_obj_.shininess_tex);
				}
				break;

			case BUMP_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.BumpTexture(mat_id, properties_obj_.bump_tex);
				}
				break;

			case HEIGHT_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.HeightTexture(mat_id, properties_obj_.height_tex);
				}
				break;

			case EMIT_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.EmitTexture(mat_id, properties_obj_.emit_tex);
				}
				break;

			case OPACITY_TEX_ORDER:
				if (selected_mesh_index_ > 0)
				{
					uint mat_id = core_.MaterialID(selected_mesh_index_ - 1);
					core_.OpacityTexture(mat_id, properties_obj_.opacity_tex);
				}
				break;

			default:
				break;
			}
		}

		private MtlEditorCore core_;
		private DateTime last_time_;
		private double frame_;
		private ModelPropertyTypes properties_obj_;
		private uint selected_mesh_index_;
	}

	public class MeshItemsSource : IItemsSource
	{
		static public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection items;

		public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
		{
			return items;
		}
	}
}
