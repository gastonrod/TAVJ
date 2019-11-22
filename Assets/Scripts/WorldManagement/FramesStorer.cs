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

        public FramesStorer()
        {
            _frames = new Frame[_bufferSize];
        }

        public Frame GetNextFrame()
        {
            Frame frame = _frames[0];
            return frame;
        }

        public void StoreFrame(byte[] snapshot)
        {
            Frame frame = new Frame(snapshot);
            if (_frames[0] == null)
            {
                _frames[0] = frame;
            }

            if (_frames[1] == null)
            {
                _frames[1] = frame;
            }
            _frames[2] = frame;
        }
        private String FramesToString()
        {
            String s = "Frames: " + _mustInterpolate + "\n";
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] == null)
                    return s;
//                s += "i: " + Utils.FrameToString(_frames[i]) + "\n";
            }
            return s;
        }
    }
}