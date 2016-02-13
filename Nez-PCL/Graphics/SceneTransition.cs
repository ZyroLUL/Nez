﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Threading;
using Nez.Tweens;


namespace Nez
{
	/// <summary>
	/// SceneTransition is used to transition from one Scene to another with an effect
	/// </summary>
	public abstract class SceneTransition
	{
		/// <summary>
		/// contains the last render of the previous Scene. Can be used to obscure the screen while loading a new Scene.
		/// </summary>
		public RenderTarget2D previousSceneRender;

		/// <summary>
		/// if true, Nez will render the previous scene into previousSceneRender so that you can use it with your transition 
		/// </summary>
		public bool wantsPreviousSceneRender;

		/// <summary>
		/// function that should return the newly loaded scene
		/// </summary>
		protected Func<Scene> sceneLoadAction;

		/// <summary>
		/// used internally to decide if the previous Scene should render into previousSceneRender. Does double duty to ensure that the
		/// render only happens once.
		/// </summary>
		/// <value><c>true</c> if has previous scene render; otherwise, <c>false</c>.</value>
		internal bool hasPreviousSceneRender
		{
			get
			{
				if( !_hasPreviousSceneRender )
				{
					_hasPreviousSceneRender = true;
					return false;
				}

				return true;
			}
		}
		bool _hasPreviousSceneRender;



		public SceneTransition( Func<Scene> sceneLoadAction, bool wantsPreviousSceneRender = true )
		{
			this.sceneLoadAction = sceneLoadAction;
			this.wantsPreviousSceneRender = wantsPreviousSceneRender;

			// create a RenderTarget if we need to for later
			if( wantsPreviousSceneRender )
				previousSceneRender = new RenderTarget2D( Core.graphicsDevice, Screen.width, Screen.height, false, Screen.backBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents );
		}


		/// <summary>
		/// called after the previousSceneRender occurs for the first (and only) time. At this point you can load your new Scene after
		/// yielding one frame (so the first render call happens before scene loading).
		/// </summary>
		public virtual IEnumerator onBeginTransition()
		{
			yield return null;
			Core.scene = sceneLoadAction();
			transitionComplete();
		}


		public virtual void render()
		{
			Core.graphicsDevice.SetRenderTarget( null );
			Graphics.instance.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.Opaque, Core.defaultSamplerState );
			Graphics.instance.spriteBatch.Draw( previousSceneRender, Vector2.Zero, Color.White );
			Graphics.instance.spriteBatch.End();
		}


		/// <summary>
		/// this should be called when your transition is complete and the new Scene has been set. It will clean up
		/// </summary>
		protected virtual void transitionComplete()
		{
			Core._instance._sceneTransition = null;

			if( previousSceneRender != null )
			{
				previousSceneRender.Dispose();
				previousSceneRender = null;
			}
		}


		/// <summary>
		/// the most common type of transition seems to be one that ticks progress from 0 - 1. This method takes care of that for you
		/// if your transition needs to have a _progress property ticked after the scene loads.
		/// </summary>
		/// <param name="duration">duration</param>
		/// <param name="reverseDirection">if true, _progress will go from 1 to 0. If false, it goes form 0 to 1</param>
		public IEnumerator tickEffectProgressProperty( Effect effect, float duration, bool reverseDirection = false )
		{
			var start = reverseDirection ? 1f : 0f;
			var end = reverseDirection ? 0f : 1f;

			var elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				var step = MathHelper.Lerp( start, end, Mathf.pow( elapsed / duration, 2f ) );
				effect.Parameters["_progress"].SetValue( step );

				yield return null;
			}
		}

	}
}

