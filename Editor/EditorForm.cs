using GS_PatEditor.Editor.Exporters.Player;
using GS_PatEditor.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    public partial class EditorForm : Form
    {
        public static void ShowEditorForm(Pat.Project proj)
        {
            using (var frm = new EditorForm())
            {
                using (var editor = new Editor(proj))
                {
                    Program.EditorForBackup = editor;

                    frm._Editor = editor;

                    editor.SoundEffects = new ProjectSoundEffectCache(editor, frm.Handle);

                    editor.AnimationFramesUI.Init(frm.animationFrames);
                    editor.PreviewWindowUI.Init(frm.previewWindow);
                    editor.AnimationListUI.Init(frm.animations);

                    #region init clipboards
                    frm._ClipboardPhysical = new ClipboardUIProvider(editor.PreviewWindowUI.PhysicalEditing)
                    {
                        New = new ClipboardUIElementToolstripItem(frm.newPhysicalToolStripMenuItem),
                        Cut = new ClipboardUIElementToolstripItem(frm.cutPhysicalToolStripMenuItem),
                        Copy = new ClipboardUIElementToolstripItem(frm.copyPhysicalToolStripMenuItem),
                        Paste = new ClipboardUIElementToolstripItem(frm.pastePhysicalToolStripMenuItem),
                        Delete = new ClipboardUIElementToolstripItem(frm.deletePhysicalToolStripMenuItem),
                    };
                    frm._ClipboardHit = new ClipboardUIProvider(editor.PreviewWindowUI.HitEditing)
                    {
                        New = new ClipboardUIElementToolstripItem(frm.newHitToolStripMenuItem),
                        Cut = new ClipboardUIElementToolstripItem(frm.cutHitToolStripMenuItem),
                        Copy = new ClipboardUIElementToolstripItem(frm.copyHitToolStripMenuItem),
                        Paste = new ClipboardUIElementToolstripItem(frm.pasteHitToolStripMenuItem),
                        Delete = new ClipboardUIElementToolstripItem(frm.deleteHitToolStripMenuItem),
                    };
                    frm._ClipboardAttack = new ClipboardUIProvider(editor.PreviewWindowUI.AttackEditing)
                    {
                        New = new ClipboardUIElementToolstripItem(frm.newAttackToolStripMenuItem),
                        Cut = new ClipboardUIElementToolstripItem(frm.cutAttackToolStripMenuItem),
                        Copy = new ClipboardUIElementToolstripItem(frm.copyAttackToolStripMenuItem),
                        Paste = new ClipboardUIElementToolstripItem(frm.pasteAttackToolStripMenuItem),
                        Delete = new ClipboardUIElementToolstripItem(frm.deleteAttackToolStripMenuItem),
                    };
                    frm._ClipboardFrame = new ClipboardUIProvider(editor.AnimationFramesUI)
                    {
                        Cut = new ClipboardUIElementToolstripItem(frm.toolStripMenuItemCutFrame),
                        Copy = new ClipboardUIElementToolstripItem(frm.toolStripMenuItemCopyFrame),
                        Paste = new ClipboardUIElementToolstripItem(frm.toolStripMenuItemPasteFrame),
                        Delete = new ClipboardUIElementToolstripItem(frm.toolStripMenuItemDeleteFrame),
                    };
                    #endregion

                    #region init edit menu visible groups
                    frm._GroupEditPhysical = new VisibleGroup(
                        frm.physicalToolStripMenuItem1,
                        frm.newPhysicalToolStripMenuItem,
                        frm.cutPhysicalToolStripMenuItem,
                        frm.copyPhysicalToolStripMenuItem,
                        frm.pastePhysicalToolStripMenuItem,
                        frm.deletePhysicalToolStripMenuItem
                        );
                    frm._GroupEditHit = new VisibleGroup(
                        frm.hitToolStripMenuItem,
                        frm.newHitToolStripMenuItem,
                        frm.cutHitToolStripMenuItem,
                        frm.copyHitToolStripMenuItem,
                        frm.pasteHitToolStripMenuItem,
                        frm.deleteHitToolStripMenuItem
                        );
                    frm._GroupEditAttack = new VisibleGroup(
                        frm.attackToolStripMenuItem2,
                        frm.newAttackToolStripMenuItem,
                        frm.cutAttackToolStripMenuItem,
                        frm.copyAttackToolStripMenuItem,
                        frm.pasteAttackToolStripMenuItem,
                        frm.deleteAttackToolStripMenuItem
                        );
                    #endregion

                    #region init tool bar groups
                    frm._GroupToolAnimationList = new VisibleGroup(
                        frm.toolStripButtonNew,
                        frm.toolStripButtonOpen,
                        frm.toolStripButtonSave,
                        frm.toolStripButtonSaveAs,
                        frm.toolStripButtonDirectories,
                        frm.toolStripSeparator13,
                        frm.toolStripButtonImport,
                        frm.toolStripSeparator14,
                        frm.toolStripButtonExporter,
                        frm.toolStripButtonExport,
                        frm.toolStripSeparator7,
                        frm.toolStripButtonNewAnimation,
                        frm.toolStripButtonRemoveAnimation,
                        frm.toolStripButtonEditAnimation,
                        frm.toolStripButtonCopyAnimation,
                        frm.toolStripButtonAnimationProperty
                        );
                    frm._GroupToolAnimation = new VisibleGroup(
                        frm.toolStripExpandAll, frm.toolStripCollapseAll,
                        frm.toolStripSeparator1,
                        frm.toolStripButtonToolCursor, frm.toolStripButtonToolMove,
                        frm.toolStripButtonToolPhysics, frm.toolStripButtonToolHit,
                        frm.toolStripButtonToolAttack, frm.toolStripButtonToolPoint,
                        frm.toolStripSeparator2,
                        frm.toolStripSplitBoxVisible,
                        frm.toolStripSeparator3,
                        frm.toolStripSplitEdit,
                        frm.toolStripSplitButtonKeyFrame,
                        frm.toolStripSeparator6,
                        frm.toolStripButtonPlay,
                        frm.toolStripButtonEditAction,
                        frm.toolStripSeparator12,
                        frm.toolStripButtonBack
                    );
                    frm._GroupToolImageList = new VisibleGroup(new ToolStripButton[0]);

                    frm.SetupToolbarEnabled();
                    #endregion

                    editor.AnimationListUI.SelectedChange += frm.SetupToolbarEnabled;
                    editor.FrameReset += delegate()
                    {
                        var action = editor.CurrentAction;
                        var seg = editor.CurrentSegment;
                        if (action != null && seg != null && seg.Frames.Count > 0)
                        {
                            var isKeyFrame = editor.CurrentFrame == seg.Frames[0];
                            var isLoop = isKeyFrame ? editor.CurrentSegment.IsLoop : false;
                            frm.keyFrameToolStripMenuItem.Enabled = true;
                            frm.keyFrameToolStripMenuItem.Checked = isKeyFrame;

                            frm.editDamageToolStripMenuItem.Enabled = isKeyFrame;
                            frm.loopToolStripMenuItem.Enabled = isKeyFrame;
                            frm.loopToolStripMenuItem.Checked = isLoop;
                        }
                        else
                        {
                            frm.keyFrameToolStripMenuItem.Enabled = false;
                            frm.keyFrameToolStripMenuItem.Checked = false;

                            frm.editDamageToolStripMenuItem.Enabled = false;
                            frm.loopToolStripMenuItem.Enabled = false;
                            frm.loopToolStripMenuItem.Checked = false;
                        }
                    };

                    editor.EditModeChanged += frm.OnEditModeChanged;
                    editor.PreviewModeChanged += frm.OnPreviewModeChanged;

                    editor.CurrentUISwitched += delegate()
                    {
                        switch (editor.CurrentUI)
                        {
                            case EditorUI.AnimationList:
                                frm.ChangeActivePanel(0);
                                break;
                            case EditorUI.Animation:
                                frm.ChangeActivePanel(1);
                                editor.EditMode = FrameEditMode.None;
                                frm.ResetPreviewPosition(1.0f);
                                frm.OnEditModeChanged();
                                break;
                        }
                        frm.OnPreviewModeChanged();
                    };
                    editor.CurrentUI = EditorUI.AnimationList;

                    frm.Show();
                    frm.RunRenderLoop();

                    Program.EditorForBackup = null;
                }
            }
        }

        private class VisibleGroup
        {
            private readonly ToolStripItem[] _Items;
            private readonly ToolStripButton[] _Buttons;

            public VisibleGroup(params ToolStripItem[] ctrls)
            {
                _Items = ctrls;
                _Buttons = new ToolStripButton[0];
            }

            public VisibleGroup(params ToolStripButton[] ctrls)
            {
                _Items = new ToolStripItem[0];
                _Buttons = ctrls;
            }

            public bool Visible
            {
                set
                {
                    foreach (var c in _Items)
                    {
                        c.Visible = value;
                    }
                    foreach (var c in _Buttons)
                    {
                        c.Visible = value;
                    }
                }
            }
        }

        private Editor _Editor;
        private ClipboardUIProvider _ClipboardPhysical;
        private ClipboardUIProvider _ClipboardHit;
        private ClipboardUIProvider _ClipboardAttack;
        private ClipboardUIProvider _ClipboardFrame;

        private VisibleGroup _GroupEditPhysical, _GroupEditHit, _GroupEditAttack;
        private VisibleGroup _GroupToolAnimationList, _GroupToolAnimation, _GroupToolImageList;

        private RecentFileList _RecentFileList;

        public EditorForm()
        {
            InitializeComponent();
            _RecentFileList = new RecentFileList(toolStripButtonOpen);
            _RecentFileList.OpenFile += OpenFile;
        }

        private void RunRenderLoop()
        {
            int count = 0;
            var clock = new System.Diagnostics.Stopwatch();
            clock.Start();
            SharpDX.Windows.RenderLoop.Run(this, delegate()
            {
                if (clock.ElapsedMilliseconds >= 1000 * 5)
                {
                    var fps = (count * 1000.0f / clock.ElapsedMilliseconds);
                    clock.Restart();
                    count = 0;
                }
                _Editor.PreviewWindowUI.Refresh();
                ++count;
            });
        }

        private void toolStripCollapseAll_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.CollapseAll();
        }

        private void toolStripExpandAll_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.ExpandAll();
        }

        private void OnEditModeChanged()
        {
            _GroupEditPhysical.Visible = false;
            _GroupEditHit.Visible = false;
            _GroupEditAttack.Visible = false;
            switch (_Editor.EditMode)
            {
                case FrameEditMode.Physical:
                    _GroupEditPhysical.Visible = true;
                    break;
                case FrameEditMode.Hit:
                    _GroupEditHit.Visible = true;
                    break;
                case FrameEditMode.Attack:
                    _GroupEditAttack.Visible = true;
                    break;
            }
            UpdateToolButtonsToolChecked();
        }

        private void OnPreviewModeChanged()
        {
            toolStripButtonPlay.Enabled = true;
            if (_Editor.PreviewMode == FramePreviewMode.Pause)
            {
                toolStripButtonPlay.Text = EditorFormCodeRes.Play;
            }
            else if (_Editor.PreviewMode == FramePreviewMode.Play)
            {
                toolStripButtonPlay.Text = EditorFormCodeRes.Stop;
            }
            else
            {
                toolStripButtonPlay.Enabled = false;
            }
        }

        private void SetupToolbarEnabled()
        {
            if (_Editor.Project.IsEmptyProject)
            {
                foreach (var item in toolStrip1.Items)
                {
                    if (item is ToolStripItem)
                    {
                        ((ToolStripItem)item).Enabled = false;
                    }
                }

                toolStripButtonNew.Enabled = true;
                toolStripButtonOpen.Enabled = true;
            }
            else
            {
                foreach (var item in toolStrip1.Items)
                {
                    if (item is ToolStripItem)
                    {
                        ((ToolStripItem)item).Enabled = true;
                    }
                }

                var enabled = _Editor.AnimationListUI.HasSelected;
                toolStripButtonRemoveAnimation.Enabled = enabled;
                toolStripButtonEditAnimation.Enabled = enabled;
                toolStripButtonCopyAnimation.Enabled = enabled;
                toolStripButtonAnimationProperty.Enabled = enabled;
            }
        }

        private void ChangeActivePanel(int panel)
        {
            switch (panel)
            {
                case 0:
                    _GroupToolAnimationList.Visible = true;
                    _GroupToolAnimation.Visible = false;
                    _GroupToolImageList.Visible = false;
                    panelAnimations.Visible = true;
                    panelAnimationEdit.Visible = false;
                    break;
                case 1:
                    _GroupToolAnimationList.Visible = false;
                    _GroupToolAnimation.Visible = true;
                    _GroupToolImageList.Visible = false;
                    panelAnimations.Visible = false;
                    panelAnimationEdit.Visible = true;
                    break;
            }

            SetupToolbarEnabled();
        }

        private void CenterPreview()
        {
            previewWindow.Left = (panelFramePreviewScroll.ClientSize.Width - previewWindow.Width) / 2;
            previewWindow.Top = (panelFramePreviewScroll.ClientSize.Height - previewWindow.Height) / 2;
        }

        private void ResetPreviewPosition(float scale)
        {
            _Editor.PreviewWindowUI.PreviewMoving.ResetScale(scale);
            CenterPreview();
        }

        private void UpdateToolButtonsToolChecked()
        {
            toolStripButtonToolCursor.CheckState = CheckState.Unchecked;
            toolStripButtonToolMove.CheckState = CheckState.Unchecked;
            toolStripButtonToolPhysics.CheckState = CheckState.Unchecked;
            toolStripButtonToolHit.CheckState = CheckState.Unchecked;
            toolStripButtonToolAttack.CheckState = CheckState.Unchecked;
            toolStripButtonToolPoint.CheckState = CheckState.Unchecked;

            switch (_Editor.EditMode)
            {
                case FrameEditMode.None:
                    toolStripButtonToolCursor.CheckState = CheckState.Checked;
                    break;
                case FrameEditMode.Move:
                    toolStripButtonToolMove.CheckState = CheckState.Checked;
                    break;
                case FrameEditMode.Physical:
                    toolStripButtonToolPhysics.CheckState = CheckState.Checked;
                    break;
                case FrameEditMode.Hit:
                    toolStripButtonToolHit.CheckState = CheckState.Checked;
                    break;
                case FrameEditMode.Attack:
                    toolStripButtonToolAttack.CheckState = CheckState.Checked;
                    break;
                case FrameEditMode.Point:
                    toolStripButtonToolPoint.CheckState = CheckState.Checked;
                    break;
            }
        }

        private void toolStripButtonToolCursor_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.None;
            UpdateToolButtonsToolChecked();
        }

        private void toolStripButtonToolMove_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.Move;
            UpdateToolButtonsToolChecked();
        }

        private void toolStripButtonToolPhysics_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.Physical;
            UpdateToolButtonsToolChecked();
        }

        private void toolStripButtonToolHit_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.Hit;
            UpdateToolButtonsToolChecked();
        }

        private void toolStripButtonToolAttack_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.Attack;
            UpdateToolButtonsToolChecked();
        }

        private void toolStripButtonToolPoint_Click(object sender, EventArgs e)
        {
            _Editor.EditMode = FrameEditMode.Point;
            UpdateToolButtonsToolChecked();
        }

        private void physicalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            physicalToolStripMenuItem.Checked = !physicalToolStripMenuItem.Checked;
            _Editor.PhysicalBoxVisible = physicalToolStripMenuItem.Checked;
        }

        private void axisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axisToolStripMenuItem.Checked = !axisToolStripMenuItem.Checked;
            _Editor.AxisVisible = axisToolStripMenuItem.Checked;
        }

        private void hitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            hitToolStripMenuItem1.Checked = !hitToolStripMenuItem1.Checked;
            _Editor.HitBoxVisible = hitToolStripMenuItem1.Checked;
        }

        private void attackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            attackToolStripMenuItem.Checked = !attackToolStripMenuItem.Checked;
            _Editor.AttackBoxVisible = attackToolStripMenuItem.Checked;
        }

        private void pointVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pointVisibleToolStripMenuItem.Checked = !pointVisibleToolStripMenuItem.Checked;
            _Editor.PointVisible = pointVisibleToolStripMenuItem.Checked;
        }

        private void borderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            borderToolStripMenuItem.Checked = !borderToolStripMenuItem.Checked;
            _Editor.BorderVisible = borderToolStripMenuItem.Checked;
        }

        private void toolStripSplitEdit_DropDownOpening(object sender, EventArgs e)
        {
            _ClipboardPhysical.UpdateEnable();
            _ClipboardHit.UpdateEnable();
            _ClipboardAttack.UpdateEnable();
        }

        private void resetScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPreviewPosition(1.0f);
        }

        private void scale200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPreviewPosition(2.0f);
        }

        private void scale300ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPreviewPosition(3.0f);
        }

        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            _Editor.PreviewMode = FramePreviewMode.Pause;
            _Editor.CurrentUI = EditorUI.AnimationList;
        }

        private void toolStripButtonEditAnimation_Click(object sender, EventArgs e)
        {
            _Editor.AnimationListUI.EditCurrent();
        }

        private void toolStripButtonCopyAnimation_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(EditorFormCodeRes.CopyActionConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.None) == DialogResult.Yes)
            {
                _Editor.AnimationListUI.CopyCurrent();
            }
        }

        private void toolStripButtonRemoveAnimation_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(EditorFormCodeRes.RemoveActionConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                _Editor.AnimationListUI.RemoveCurrent();
            }
        }

        private void toolStripButtonNewAnimation_Click(object sender, EventArgs e)
        {
            _Editor.AnimationListUI.AddNew();
        }

        private void toolStripButtonAnimationProperty_Click(object sender, EventArgs e)
        {
            _Editor.AnimationListUI.EditProperty();
        }

        private void keyFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (keyFrameToolStripMenuItem.Checked)
            {
                if (MessageBox.Show(EditorFormCodeRes.RemoveKeyFrameConfirm,
                        EditorFormCodeRes.MsgBoxTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    _Editor.AnimationFramesUI.SetCurrentToNormalFrame();
                }
            }
            else
            {
                if (MessageBox.Show(EditorFormCodeRes.CreateKeyFrameConfirm, EditorFormCodeRes.MsgBoxTitle,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _Editor.AnimationFramesUI.SetCurrentToKeyFrame();
                }
            }
        }

        private void loopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.SwitchCurrentLoop();
        }

        private void toolStripMenuItemEditFrame_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.ShowEditFrameForm();
        }

        private void toolStripMenuItemSelectImage_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.ShowSelectImageForm();
        }

        private void toolStripMenuItemAddImages_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.ShowAddImagesForm();
        }

        private void toolStripMenuItemAddFrame_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.InsertNewFrameBefore();
        }

        private bool _FrameMenuOpeningFlag = false;
        private void toolStripSplitButtonKeyFrame_DropDownOpening(object sender, EventArgs e)
        {
            _FrameMenuOpeningFlag = true;

            _ClipboardFrame.UpdateEnable();

            //update cancellable selected
            //TODO make it clean

            var cancelLevel = _Editor.AnimationFramesUI.CancelLevel;
            toolStripComboBoxCancelLevel.SelectedIndex = cancelLevel;
            toolStripComboBoxCancelLevel.Enabled = cancelLevel != -1;

            var enabled = _Editor.AnimationFramesUI.CancellableEnabled;
            jumpCancellableStripMenuItem.Enabled = enabled;
            skillCancellableToolStripMenuItem.Enabled = enabled;

            jumpCancellableStripMenuItem.Checked = _Editor.AnimationFramesUI.JumpCancellable;
            skillCancellableToolStripMenuItem.Checked = _Editor.AnimationFramesUI.SkillCancellable;

            _FrameMenuOpeningFlag = false;
        }

        private void editDamageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _Editor.AnimationFramesUI.ShowEditDamageForm();
        }

        private void jumpCancellableStripMenuItem_Click(object sender, EventArgs e)
        {
            jumpCancellableStripMenuItem.Checked = !jumpCancellableStripMenuItem.Checked;
            _Editor.AnimationFramesUI.JumpCancellable = jumpCancellableStripMenuItem.Checked;
        }

        private void skillCancellableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            skillCancellableToolStripMenuItem.Checked = !skillCancellableToolStripMenuItem.Checked;
            _Editor.AnimationFramesUI.SkillCancellable = skillCancellableToolStripMenuItem.Checked;
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            if (_Editor.Project.IsEmptyProject || MessageBox.Show(EditorFormCodeRes.NewProjectConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var dialog = new CreateProjectForm();
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                _Editor.Project = ProjectGenerater.GenerateEmpty(dialog.ImagePath);
                _Editor.Project.ImageList.ReloadPaletteList();

                SetupToolbarEnabled();
            }
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            if (_Editor.Project.IsEmptyProject || MessageBox.Show(EditorFormCodeRes.OpenProjectConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    OpenFile(openFileDialog1.FileName);
                }
            }
        }

        private void OpenFile(string file)
        {
            _RecentFileList.AddToRecentList(file);

            Pat.Project proj = null;
            if (System.IO.Path.GetExtension(file) == ".pat")
            {
                proj = ProjectGenerater.Generate(file);
            }
            else if (System.IO.Path.GetExtension(file) == ".patproj")
            {
                proj = ProjectSerializer.OpenProject(file);
            }
            else
            {
                MessageBox.Show(EditorFormCodeRes.ErrorUnknownProjectFormat);
            }

            if (proj != null)
            {
                _Editor.Project = proj;
            }

            SetupToolbarEnabled();

            openFileDialog1.FileName = String.Empty;
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (_Editor.Project.FilePath != null)
            {
                ProjectSerializer.SaveProject(_Editor.Project, _Editor.Project.FilePath);
            }
            else if (saveFileDialogSave.ShowDialog() == DialogResult.OK)
            {
                ProjectSerializer.SaveProject(_Editor.Project, saveFileDialogSave.FileName);
                _Editor.Project.FilePath = saveFileDialogSave.FileName;

                saveFileDialogSave.FileName = String.Empty;
            }
        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject && _Editor.Project.Exporter != null)
            {
                if (_Editor.Project.LastExportDirectory != null &&
                    Directory.Exists(_Editor.Project.LastExportDirectory))
                {
                    saveFileDialogExport.InitialDirectory = _Editor.Project.LastExportDirectory;
                }
                if (saveFileDialogExport.ShowDialog() == DialogResult.OK)
                {
                    var proj = _Editor.Project;

                    var file = saveFileDialogExport.FileName;
                    proj.LastExportDirectory = Path.GetDirectoryName(file);
                    proj.LastExportFileName = Path.GetFileName(file);

                    proj.Exporter.InitExporter(proj, file);
                    proj.Exporter.Export(proj);
                    proj.Exporter.FinishExporter();

                    proj.PostExportScript.Execute();

                    saveFileDialogExport.FileName = String.Empty;
                }
            }
        }

        private void panelFramePreviewScroll_Resize(object sender, EventArgs e)
        {
            CenterPreview();
        }

        private void toolStripButtonSaveAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialogSave.ShowDialog() == DialogResult.OK)
            {
                ProjectSerializer.SaveProject(_Editor.Project, saveFileDialogSave.FileName);
                _Editor.Project.FilePath = saveFileDialogSave.FileName;

                saveFileDialogSave.FileName = String.Empty;
            }
        }

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            if (toolStripButtonPlay.Text == EditorFormCodeRes.Play)
            {
                toolStripButtonPlay.Text = EditorFormCodeRes.Stop;
                _Editor.PreviewMode = FramePreviewMode.Play;
            }
            else if (toolStripButtonPlay.Text == EditorFormCodeRes.Stop)
            {
                toolStripButtonPlay.Text = EditorFormCodeRes.Play;
                _Editor.PreviewMode = FramePreviewMode.Pause;
            }
        }

        private void toolStripButtonEditAction_Click(object sender, EventArgs e)
        {
            _Editor.ShowActionEditForm();
        }

        private void toolStripButtonExporter_DropDownOpening(object sender, EventArgs e)
        {
            editExporterToolStripMenuItem.Enabled = false;
            removeExporterToolStripMenuItem.Enabled = false;
            createExporterToolStripMenuItem.Enabled = false;

            if (!_Editor.Project.IsEmptyProject)
            {
                if (_Editor.Project.Exporter != null)
                {
                    editExporterToolStripMenuItem.Enabled = true;
                    removeExporterToolStripMenuItem.Enabled = true;
                }
                else
                {
                    createExporterToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void playerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject)
            {
                _Editor.Project.Exporter = new PlayerExporter();
            }
        }

        private void removeExporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject)
            {
                if (MessageBox.Show(EditorFormCodeRes.RemoveExporterConfirm, EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    _Editor.Project.Exporter = null;
                }
            }
        }

        private void editExporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject && _Editor.Project.Exporter != null)
            {
                _Editor.Project.Exporter.ShowOptionDialog(_Editor.Project);
            }
        }

        private void toolStripButtonDirectories_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject)
            {
                var oldList = new List<Pat.ProjectDirectoryDesc>(_Editor.Project.Settings.Directories);
                var dialog = new ProjectDirectoryEditForm(_Editor.Project.Settings, true);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _Editor.Project.ImageList.ReloadAllResources();
                    _Editor.Project.ImageList.ReloadPaletteList();
                    _Editor.AnimationListUI.Activate();
                }
                else
                {
                    _Editor.Project.Settings.Directories = oldList;
                }
            }
        }

        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {
            var dialog = new ImportPatAnimationForm(_Editor.Project);
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var segments = dialog.ImportedSegments;
                if (segments != null)
                {
                    int id = 1;
                    {
                        var list = _Editor.Project.Actions;
                        while (list.Any(a => a.ActionID == EditorFormCodeRes.ImportedAnimationPrefix + id))
                        {
                            ++id;
                        }
                    }
                    var action = new Pat.Action()
                    {
                        ActionID = EditorFormCodeRes.ImportedAnimationPrefix + id,
                        ImageID = null,
                        Segments = segments,
                    };

                    //TODO move to action
                    if (action.Segments.Count > 0 && action.Segments[0].Frames.Count > 0)
                    {
                        action.ImageID = action.Segments[0].Frames[0].ImageID;
                    }

                    _Editor.Project.Actions.Add(action);

                    _Editor.Project.ImageList.ReloadAllResources();
                    _Editor.AnimationListUI.Activate();
                }
            }
        }

        private void EditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_Editor.CurrentUI == EditorUI.Animation)
            {
                //QE switch frame
                if (e.KeyCode == Keys.Q && e.Modifiers == Keys.None)
                {
                    _Editor.AnimationFramesUI.SelectLastKeyGrid();
                }
                else if (e.KeyCode == Keys.E && e.Modifiers == Keys.None)
                {
                    _Editor.AnimationFramesUI.SelectNextKeyGrid();
                }
                //arrow accurate move
                else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.None)
                {
                    _Editor.PreviewWindowUI.InvokeAccurateMove(0, -1);
                }
                else if (e.KeyCode == Keys.Down && e.Modifiers == Keys.None)
                {
                    _Editor.PreviewWindowUI.InvokeAccurateMove(0, 1);
                }
                else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.None)
                {
                    _Editor.PreviewWindowUI.InvokeAccurateMove(-1, 0);
                }
                else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.None)
                {
                    _Editor.PreviewWindowUI.InvokeAccurateMove(1, 0);
                }
                //clipboard shortcut
                else if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
                {
                    _Editor.PreviewWindowUI.InvokeShortcutEvent(ShortcutEventType.Cut);
                }
                else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
                {
                    _Editor.PreviewWindowUI.InvokeShortcutEvent(ShortcutEventType.Copy);
                }
                else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
                {
                    _Editor.PreviewWindowUI.InvokeShortcutEvent(ShortcutEventType.Paste);
                }
                else if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
                {
                    _Editor.PreviewWindowUI.InvokeShortcutEvent(ShortcutEventType.Delete);
                }
            }
        }

        private void addReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject && _Editor.CurrentUI == EditorUI.Animation)
            {
                if (_Editor.CurrentFrame != null)
                {
                    _Editor.PreviewWindowUI.ReferenceList.Add(new Panels.FrameReferenceInfo
                    {
                        Action = _Editor.CurrentAction,
                        Frame = _Editor.CurrentFrame,
                    });
                }
            }
        }

        private void editReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject && _Editor.CurrentUI == EditorUI.Animation)
            {
                var dialog = new FrameReferenceEditForm(_Editor.Project);
                dialog.List = _Editor.PreviewWindowUI.ReferenceList;
                dialog.OpacityValue = _Editor.PreviewWindowUI.ReferenceOpacity;

                dialog.ShowDialog();
                
                _Editor.PreviewWindowUI.ReferenceList.Clear();
                _Editor.PreviewWindowUI.ReferenceList.AddRange(dialog.List);
                _Editor.PreviewWindowUI.ReferenceOpacity = dialog.OpacityValue;
            }
        }

        private void toolStripComboBoxCancelLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_FrameMenuOpeningFlag)
            {
                _Editor.AnimationFramesUI.CancelLevel = toolStripComboBoxCancelLevel.SelectedIndex;
            }
        }

        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Editor.Project.IsEmptyProject)
            {
                return;
            }
            switch (MessageBox.Show(EditorFormCodeRes.ExitConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.None))
            {
                case DialogResult.Yes:
                    toolStripButtonSave_Click(null, EventArgs.Empty);
                    break;
                case DialogResult.No:
                    break;
                case DialogResult.Cancel:
                default:
                    e.Cancel = true;
                    break;
            }
        }

        private void exportToLastLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_Editor.Project.IsEmptyProject && _Editor.Project.Exporter != null)
            {
                if (_Editor.Project.LastExportDirectory != null &&
                    Directory.Exists(_Editor.Project.LastExportDirectory) &&
                    !String.IsNullOrEmpty(_Editor.Project.LastExportFileName))
                {
                    var proj = _Editor.Project;

                    var file = Path.Combine(proj.LastExportDirectory, proj.LastExportFileName);

                    proj.Exporter.InitExporter(proj, file);
                    proj.Exporter.Export(proj);
                    proj.Exporter.FinishExporter();

                    proj.PostExportScript.Execute(delegate()
                    {
                        MessageBox.Show(EditorFormCodeRes.AutoExportFinished, EditorFormCodeRes.MsgBoxTitle);
                    });
                }
            }
        }

        private void postExportActionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new GS_PatEditor.Editor.Exporters.PostExportScriptForm();
            var s = _Editor.Project.PostExportScript;
            dialog.BatFile = s.BatFile;
            dialog.WorkingDirectory = s.Directory;
            dialog.ScriptEnabled = s.Enabled;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                s.BatFile = dialog.BatFile;
                s.Directory = dialog.WorkingDirectory;
                s.Enabled = dialog.ScriptEnabled;
            }
        }

        private void alignAllFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(EditorFormCodeRes.AlignAllFramesConfirm, EditorFormCodeRes.MsgBoxTitle,
                MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                var seg = _Editor.CurrentSegment;
                var frame = _Editor.CurrentFrame;
                var imageFrame = _Editor.Project.ImageList.GetImage(frame.ImageID);
                var x = -frame.OriginX + imageFrame.Width * frame.ScaleX / 200;
                var y = -frame.OriginY + imageFrame.Height * frame.ScaleY / 100;
                foreach (var f in seg.Frames)
                {
                    var image2 = _Editor.Project.ImageList.GetImage(f.ImageID);
                    var x2 = -f.OriginX + image2.Width * f.ScaleX / 200;
                    var y2 = -f.OriginY + image2.Height * f.ScaleY / 100;
                    f.OriginX -= x - x2;
                    f.OriginY -= y - y2;
                }
            }
        }
    }
}
