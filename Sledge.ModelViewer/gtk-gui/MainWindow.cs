
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	
	private global::Gtk.Action FileAction;
	
	private global::Gtk.Action StuffAction;
	
	private global::Gtk.Action openAction;
	
	private global::Gtk.VBox vbox1;
	
	private global::Gtk.MenuBar MainMenuBar;
	
	private global::Gtk.Toolbar toolbar1;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("File"), null, null);
		this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("File");
		w1.Add (this.FileAction, null);
		this.StuffAction = new global::Gtk.Action ("StuffAction", global::Mono.Unix.Catalog.GetString ("Stuff"), null, null);
		this.StuffAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Stuff");
		w1.Add (this.StuffAction, null);
		this.openAction = new global::Gtk.Action ("openAction", null, null, "gtk-open");
		w1.Add (this.openAction, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 1;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name=\'MainMenuBar\'><menu name=\'FileAction\' action=\'FileAction\'><menu" +
		"item name=\'StuffAction\' action=\'StuffAction\'/></menu></menubar></ui>");
		this.MainMenuBar = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/MainMenuBar")));
		this.MainMenuBar.Name = "MainMenuBar";
		this.vbox1.Add (this.MainMenuBar);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.MainMenuBar]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><toolbar name=\'toolbar1\'><toolitem name=\'openAction\' action=\'openAction\'/></t" +
		"oolbar></ui>");
		this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
		this.toolbar1.Name = "toolbar1";
		this.toolbar1.ShowArrow = false;
		this.vbox1.Add (this.toolbar1);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.toolbar1]));
		w3.Position = 1;
		w3.Expand = false;
		w3.Fill = false;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 459;
		this.DefaultHeight = 430;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.StuffAction.Activated += new global::System.EventHandler (this.OpenDialog);
	}
}
