using System;
using Connections;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

namespace WorldManagement
{
    public class FramesStorer
    {
        /*
         *  Frames buffer will have the frames I've received
         *  [F0, F1, F2]
         *  Then, it will decide if it needs to send out an interpolated frame when it's asked for one, and do it
         *  on-demand.
         */
        
        private Frame[] _frames;
        private int _bufferSize = 3;
        private bool _mustInterpolate;
        private int percentageOfFrame = 0;
        private int _fps;

        public FramesStorer(int framesPerSecond = 30)
        {
            _frames = new Frame[_bufferSize];
            _fps = framesPerSecond;
        }

        public Frame GetNextFrame()
        {
            Frame frame = _frames[0];
            if (_frames[1] != null)
            {
                frame = Frame.Interpolate(_frames[0], _frames[1], (percentageOfFrame++)/(float)_fps);
                if (percentageOfFrame >= _fps)
                {
                    percentageOfFrame = 0;
                    _frames[0] = _frames[1];
                    _frames[1] = _frames[2];
                    _frames[2] = null;
                }
            }

            return frame;
        }

        public void StoreFrame(byte[] snapshot)
        {
            Frame frame = new Frame(snapshot);
            if (_frames[0] == null)
            {
                _frames[0] = frame;
            }

            if (Frame.FramesAreEqual(_frames[0], frame))
            {
                return;
            }

            if (_frames[1] == null)
            {
                _frames[1] = frame;
            }
            if (Frame.FramesAreEqual(_frames[1], frame))
            {
                return;
            }
            _frames[2] = frame;
        }
       
    }
}