using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace SpotifyClassic.Animations
{
    public class ContinuumTransition : TransitionElement
    {
        public const string ContinuumElementPropertyName = "ContinuumElement";
        public const string ContinuumModePropertyName = "Mode";

        public FrameworkElement ContinuumElement
        {
            get { return (FrameworkElement)GetValue(ContinuumElementProperty); }
            set { SetValue(ContinuumElementProperty, value); }
        }

        public ContinuumTransitionMode Mode
        {
            get { return (ContinuumTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ContinuumElementProperty =
           DependencyProperty.Register(ContinuumElementPropertyName, typeof(FrameworkElement), typeof(ContinuumTransition), new PropertyMetadata(null));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(ContinuumModePropertyName, typeof(ContinuumTransitionMode), typeof(ContinuumTransition), null);


        public ContinuumTransition() { }
        public ContinuumTransition(ContinuumTransitionMode mode)
        {
            Mode = mode;
        }
        public ContinuumTransition(ContinuumTransitionMode mode, FrameworkElement element)
        {
            Mode = mode;
            ContinuumElement = element;
        }

        public override ITransition GetTransition(UIElement element)
        {
            // 1. Cargar el Storyboard según el modo de transición actual
            Storyboard storyboard = null;
            if (Mode == ContinuumTransitionMode.ContinuumBackwardInStoryboard)
                storyboard = XamlReader.Load(ContinuumBackwardInStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.ContinuumBackwardOutStoryboard)
                storyboard = XamlReader.Load(ContinuumBackwardOutStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.ContinuumForwardInStoryboard)
                storyboard = XamlReader.Load(ContinuumForwardInStoryboard) as Storyboard;
            else if (Mode == ContinuumTransitionMode.ContinuumForwardOutStoryboard)
                storyboard = XamlReader.Load(ContinuumForwardOutStoryboard) as Storyboard;

            if (storyboard == null)
            {
                return new Transition(element, new Storyboard());
            }

            // 2. Garantizar que LayoutRoot (element) tenga un CompositeTransform asignado
            var rootElement = element as FrameworkElement;
            if (rootElement != null && !(rootElement.RenderTransform is CompositeTransform))
            {
                rootElement.RenderTransform = new CompositeTransform();
            }

            // 3. Garantizar que ContinuumElement tenga un CompositeTransform asignado (si existe)
            if (ContinuumElement != null && !(ContinuumElement.RenderTransform is CompositeTransform))
            {
                ContinuumElement.RenderTransform = new CompositeTransform();
            }

            // 4. Preparar el diccionario de objetivos para el Storyboard
            var targets = new Dictionary<string, FrameworkElement>();

            if (rootElement != null)
            {
                targets.Add("LayoutRoot", rootElement);
            }

            // Si ContinuumElement no fue asignado, asignamos un elemento simulado para evitar errores de enlace en el storyboard
            if (ContinuumElement != null)
            {
                targets.Add(ContinuumElementPropertyName, ContinuumElement);
            }
            else
            {
                // Contenedor ficticio para recibir la animación de forma segura si no se especificó ContinuumElement
                var dummyElement = new System.Windows.Controls.Canvas { RenderTransform = new CompositeTransform() };
                targets.Add(ContinuumElementPropertyName, dummyElement);
            }

            // 5. Vincular las animaciones a los elementos reales
            SetTargets(targets, storyboard);

            return new Transition(element, storyboard);
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets, Storyboard sb)
        {
            foreach (var kvp in targets)
            {
                var timelines = sb.Children.Where(t => Storyboard.GetTargetName(t) == kvp.Key);
                foreach (Timeline t in timelines)
                    Storyboard.SetTarget(t, kvp.Value);
            }
        }

        internal static readonly string ContinuumForwardOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""70"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
	        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot"">
		        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
		        <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
			        <EasingDoubleKeyFrame.EasingFunction>
				        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
			        </EasingDoubleKeyFrame.EasingFunction>
		        </EasingDoubleKeyFrame>
	        </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""73"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""225"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
			<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""ContinuumElement"" Storyboard.TargetProperty=""(UIElement.Opacity)"">
				<DiscreteDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"" />
			</DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        internal static readonly string ContinuumForwardInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""50""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-70""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""130""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.15"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        internal static readonly string ContinuumBackwardOutStoryboard =
        @"<Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""50"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.15"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        internal static readonly string ContinuumBackwardInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateX)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-70""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""ContinuumElement"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-30""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""3""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
			<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""ContinuumElement"" Storyboard.TargetProperty=""(UIElement.Opacity)"">
				<DiscreteDoubleKeyFrame KeyTime=""0:0:0"" Value=""1"" />
			</DoubleAnimationUsingKeyFrames>
	        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot"">
		        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
		        <EasingDoubleKeyFrame KeyTime=""0:0:0.15"" Value=""1"">
			        <EasingDoubleKeyFrame.EasingFunction>
				        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
			        </EasingDoubleKeyFrame.EasingFunction>
		        </EasingDoubleKeyFrame>
	        </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Duration=""0"" To=""0"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"" Storyboard.TargetName=""LayoutRoot""/>
        </Storyboard>";
    }

    public enum ContinuumTransitionMode
    {
        ContinuumForwardOutStoryboard,
        ContinuumForwardInStoryboard,
        ContinuumBackwardOutStoryboard,
        ContinuumBackwardInStoryboard
    }
}