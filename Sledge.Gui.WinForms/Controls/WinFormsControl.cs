﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using Sledge.Gui.Controls;
using Sledge.Gui.Events;
using Sledge.Gui.Interfaces;
using Binding = Sledge.Gui.Bindings.Binding;
using BindingDirection = Sledge.Gui.Bindings.BindingDirection;
using IContainer = Sledge.Gui.Interfaces.IContainer;
using MouseEventHandler = Sledge.Gui.Events.MouseEventHandler;
using Size = Sledge.Gui.Interfaces.Size;

namespace Sledge.Gui.WinForms.Controls
{
    public abstract class WinFormsControl : ITextControl
    {
        private WinFormsContainer _parent;
        private Control _control;
        private Size _preferredSize;

        public Control Control
        {
            get { return _control; }
            private set
            {
                if (_control != null) _control.Resize -= ControlResized;
                _control = value;
                if (_control != null)
                {
                    _control.Resize += ControlResized;
                    _control.AutoSize = false;
                }
            }
        }

        public IContainer Parent
        {
            get { return _parent; }
            internal set
            {
                _parent = (WinFormsContainer)value;
                OnBindingSourceChanged();
            }
        }

        public IControl Implementation
        {
            get { return this; }
        }

        public string Text
        {
            get { return _control.Text; }
            set { _control.Text = value; }
        }

        private bool _bold;
        private bool _italic;

        public int FontSize
        {
            get { return (int) _control.Font.GetHeight(); }
        }

        public bool Bold
        {
            get { return _bold; }
            set { _bold = value; } // todo
        }

        public bool Italic
        {
            get { return _italic; }
            set { _italic = value; } // todo
        }

        public bool Enabled
        {
            get { return _control.Enabled; }
            set { _control.Enabled = value; }
        }

        public bool Focused
        {
            get { return _control.Focused; }
        }

        protected WinFormsControl(Control control)
        {
            Control = control;
            Control.AutoSize = false;
            Control.MinimumSize = Control.Size = System.Drawing.Size.Empty;
        }

        #region Binding

        private readonly List<Binding> _bindings = new List<Binding>();
        private object _bindingSource;

        public object BindingSource
        {
            get { return _bindingSource; }
            set
            {
                _bindingSource = value;
                OnBindingSourceChanged();
            }
        }

        protected virtual void ApplyBinding(Binding binding)
        {
            var bs = GetInheritedBindingSource();
            if (bs == null) return;

            var prop = Control.GetType().GetProperty(binding.TargetProperty);
            if (prop != null) ApplyWinFormsBinding(binding, bs);

            var ev = GetType().GetEvent(binding.TargetProperty);
            if (ev != null) ApplyEventBinding(binding, ev, bs);
        }

        protected virtual void RemoveBinding(Binding binding)
        {
            RemoveWinFormsBinding(binding);
            RemoveEventBinding(binding);
        }

        internal virtual void OnBindingSourceChanged()
        {
            Control.DataBindings.Clear();
            foreach (var b in _bindings) ApplyBinding(b);
        }

        protected virtual object GetInheritedBindingSource()
        {
            return _bindingSource ?? (Parent == null ? null : _parent.GetInheritedBindingSource());
        }

        public Binding Bind(string property, string sourceProperty, BindingDirection direction = BindingDirection.Dual)
        {
            var b = new Binding(this, property, sourceProperty, direction);
            ApplyBinding(b);
            _bindings.Add(b);
            return b;
        }

        public void UnbindAll()
        {
            foreach (var b in _bindings) RemoveBinding(b);
            _bindings.Clear();
        }

        public void Unbind(string property)
        {
            foreach (var b in _bindings.Where(x => x.TargetProperty == property).ToList())
            {
                RemoveBinding(b);
                _bindings.Remove(b);
            }
        }

        protected System.Windows.Forms.Binding ApplyWinFormsBinding(Binding binding, object bindingSource, string targetPropertyOverride = null)
        {
            var db = Control.DataBindings.Add(targetPropertyOverride ?? binding.TargetProperty, bindingSource, binding.SourceProperty, false, DataSourceUpdateMode.OnPropertyChanged);
            db.DataSourceNullValue = null;
            binding["WinFormsBinding"] = db;
            return db;
        }

        protected void RemoveWinFormsBinding(Binding binding)
        {
            if (!binding.ContainsKey("WinFormsBinding")) return;
            Control.DataBindings.Remove((System.Windows.Forms.Binding)binding["WinFormsBinding"]);
        }

        protected void ApplyEventBinding(Binding binding, EventInfo ev, object bindingSource)
        {
            var method = bindingSource.GetType().GetMethod(binding.SourceProperty);
            if (method == null) return;

            var methodParameters = method.GetParameters();
            if (methodParameters.Length > 2) return;

            var parameters = ev.EventHandlerType.GetMethod("Invoke").GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToArray();
            var exParams = parameters.OfType<Expression>().ToArray();
            Action<object, object> act = null;

            switch (methodParameters.Length)
            {
                case 0:
                    act = (a, b) => method.Invoke(bindingSource, null);
                    break;
                case 1:
                    act = (a, b) => method.Invoke(bindingSource, new [] { b });
                    break;
                case 2:
                    act = (a, b) => method.Invoke(bindingSource, new []{ this, b});
                    break;
            }
            if (act == null) return;

            var handler = Expression.Lambda(ev.EventHandlerType, Expression.Call(Expression.Constant(act), "Invoke", null, exParams), parameters).Compile();
            ev.AddEventHandler(this, handler);
            binding["EventInfo"] = handler;
            binding["EventHandler"] = handler;
        }

        protected void ApplyManualEventBinding(Binding binding, object bindingSource, string eventName)
        {
            var prop = GetType().GetProperty(binding.TargetProperty);
            if (prop == null) return;

            var sourceProp = bindingSource.GetType().GetProperty(binding.SourceProperty);
            if (sourceProp == null) return;

            var ev = GetType().GetEvent(eventName);
            if (ev == null) return;

            var npc = bindingSource as INotifyPropertyChanged;
            if (npc != null)
            {
                PropertyChangedEventHandler act = (o, args) =>
                {
                    if (args.PropertyName == binding.SourceProperty)
                    {
                        prop.SetValue(this, sourceProp.GetValue(bindingSource, null), null);
                    }
                };
                npc.PropertyChanged += act;
            }

            Action apply = () => sourceProp.SetValue(bindingSource, prop.GetValue(this, null), null);
            var parameters = ev.EventHandlerType.GetMethod("Invoke").GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToArray();
            var handler = Expression.Lambda(ev.EventHandlerType, Expression.Call(Expression.Constant(apply), "Invoke", null), parameters).Compile();
            ev.AddEventHandler(this, handler);
            binding["EventInfo"] = handler;
            binding["EventHandler"] = handler;
        }

        protected void RemoveEventBinding(Binding binding)
        {
            if (!binding.ContainsKey("EventHandler") || !binding.ContainsKey("EventInfo")) return;
            ((EventInfo) binding["EventInfo"]).RemoveEventHandler(this, (Delegate) binding["EventHandler"]);
        }

        #endregion

        #region Events
        public event MouseEventHandler MouseDown
        {
            add { _control.MouseDown += ConvertDelegate(value, true); }
            remove { _control.MouseDown -= ConvertDelegate(value, false); }
        }

        public event MouseEventHandler MouseUp
        {
            add { _control.MouseUp += ConvertDelegate(value, true); }
            remove { _control.MouseUp -= ConvertDelegate(value, false); }
        }

        public event MouseEventHandler MouseWheel
        {
            add { _control.MouseWheel += ConvertDelegate(value, true); }
            remove { _control.MouseWheel -= ConvertDelegate(value, false); }
        }

        public event MouseEventHandler MouseMove
        {
            add { _control.MouseMove += ConvertDelegate(value, true); }
            remove { _control.MouseMove -= ConvertDelegate(value, false); }
        }

        public event MouseEventHandler MouseClick
        {
            add { _control.MouseClick += ConvertDelegate(value, true); }
            remove { _control.MouseClick -= ConvertDelegate(value, false); }
        }

        public event EventHandler MouseDoubleClick
        {
            add { _control.DoubleClick += value; }
            remove { _control.DoubleClick -= value; }
        }

        public event EventHandler MouseEnter
        {
            add { _control.MouseEnter += value; }
            remove { _control.MouseEnter -= value; }
        }

        public event EventHandler MouseLeave
        {
            add { _control.MouseLeave += value; }
            remove { _control.MouseLeave -= value; }
        }

        public event EventHandler Click
        {
            add { _control.Click += value; }
            remove { _control.Click -= value; }
        }

        public event EventHandler TextChanged
        {
            add { _control.TextChanged += value; }
            remove { _control.TextChanged -= value; }
        }

        private readonly Dictionary<Delegate, Delegate> _delegateCache = new Dictionary<Delegate, Delegate>();

        protected System.Windows.Forms.MouseEventHandler ConvertDelegate(MouseEventHandler value, bool adding)
        {
            if (!_delegateCache.ContainsKey(value)) _delegateCache.Add(value, value.ToMouseEventHandler(this));
            var val = (System.Windows.Forms.MouseEventHandler) _delegateCache[value];
            if (!adding) _delegateCache.Remove(value);
            return val;
        }
        
        #endregion

        #region Size
        protected abstract Size DefaultPreferredSize { get; }

        public virtual Size PreferredSize
        {
            get
            {
                var ps = _preferredSize;
                var dps = DefaultPreferredSize;
                return new Size(ps.Width == 0 ? dps.Width : ps.Width, ps.Height == 0 ? dps.Height : ps.Height);
            }
            set
            {
                _preferredSize = value;
                OnPreferredSizeChanged();
            }
        }

        private void ControlResized(object sender, EventArgs e)
        {
            OnActualSizeChanged();
        }

        public Size ActualSize
        {
            get { return new Size(Control.Width, Control.Height); }
        }

        public event EventHandler ActualSizeChanged;
        public event EventHandler PreferredSizeChanged;

        protected virtual void OnActualSizeChanged()
        {
            if (ActualSizeChanged != null)
            {
                ActualSizeChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPreferredSizeChanged()
        {
            if (PreferredSizeChanged != null)
            {
                PreferredSizeChanged(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}