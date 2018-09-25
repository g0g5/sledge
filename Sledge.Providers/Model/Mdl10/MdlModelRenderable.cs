﻿using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Sledge.Rendering.Engine;
using Sledge.Rendering.Interfaces;
using Sledge.Rendering.Pipelines;
using Sledge.Rendering.Renderables;
using Sledge.Rendering.Viewports;
using Veldrid;

namespace Sledge.Providers.Model.Mdl10
{
    public class MdlModelRenderable : IModelRenderable
    {
        private readonly MdlModel _model;
        public IModel Model => _model;

        private DeviceBuffer _transformsBuffer;
        private ResourceSet _transformsResourceSet;
        private Matrix4x4[] _transforms;

        private int _currentFrame;
        private long _lastFrameMillis;
        private float _interframePercent;

        public Vector3 Origin { get; set; }

        public MdlModelRenderable(MdlModel model)
        {
            _model = model;

            _transforms = new Matrix4x4[128];
            for (var i = 0; i < _transforms.Length; i++)
            {
                _transforms[i] = Matrix4x4.Identity;
            }

            _currentFrame = 0;
            _interframePercent = 0;
        }

        public void Update(long milliseconds)
        {
            const int currentSequence = 0;
            if (currentSequence >= _model.Model.Sequences.Count) return;

            var seq = _model.Model.Sequences[currentSequence];
            var targetFps = 1000 / seq.Framerate;
            var diff = milliseconds - _lastFrameMillis;

            _interframePercent += diff / targetFps;
            var skip = (int)_interframePercent;
            _interframePercent -= skip;

            _currentFrame = (_currentFrame + skip) % seq.NumFrames;
            _lastFrameMillis = milliseconds;
            
            _model.Model.GetTransforms(currentSequence, _currentFrame, _interframePercent, ref _transforms);
        }

        public void CreateResources(EngineInterface engine, RenderContext context)
        {
            _transformsBuffer = context.Device.ResourceFactory.CreateBuffer(
                new BufferDescription((uint) Unsafe.SizeOf<Matrix4x4>() * 128, BufferUsage.UniformBuffer)
            );

            _transformsResourceSet = context.Device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(context.ResourceLoader.ProjectionLayout, _transformsBuffer)
            );
        }

        public IEnumerable<ILocation> GetLocationObjects(IPipeline pipeline, IViewport viewport)
        {
            yield break;
        }

        public bool ShouldRender(IPipeline pipeline, IViewport viewport)
        {
            return pipeline.Type == PipelineType.WireframeModel || pipeline.Type == PipelineType.TexturedModel;
        }

        public void Render(RenderContext context, IPipeline pipeline, IViewport viewport, CommandList cl)
        {
            if (_transformsResourceSet == null || _transformsBuffer == null) return;

            cl.UpdateBuffer(_transformsBuffer, 0, _transforms);
            cl.SetGraphicsResourceSet(2, _transformsResourceSet);
            _model.Render(context, pipeline, viewport, cl);
        }

        public void Render(RenderContext context, IPipeline pipeline, IViewport viewport, CommandList cl, ILocation locationObject)
        {
            //
        }

        public void DestroyResources()
        {
            _transformsResourceSet?.Dispose();
            _transformsBuffer?.Dispose();

            _transformsResourceSet = null;
            _transformsBuffer = null;
        }

        public void Dispose()
        {
            //
        }
    }
}