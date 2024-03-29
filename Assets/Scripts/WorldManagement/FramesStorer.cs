﻿using System;
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
<<<<<<< Updated upstream
            Frame frame = _frames[0];
            if (_frames[1] != null)
=======
            if (_frames[1] == null)
            {
                _mustInterpolate = true;
                return _frames[0];
            }

            byte[] nextFrame;
            if (_mustInterpolate)
            {
                nextFrame = HandleInterpolation();
            }
            else
            {
                nextFrame = _frames[0];
            }
            _mustInterpolate = !_mustInterpolate;
            return nextFrame;
        }

        public bool StoreFrame(byte[] snapshot)
        {
            // First frame ever
            if (_frames[0] == null)
            {
                _frames[0] = snapshot;
                return true;
            }
            // Check if it is an already stored frame
            int i = SnapshotIsPredicted(snapshot);
            if (i != -1)
            {
                StoreFrameWithPredicted(snapshot, i);
                return true;
            }
            // If it's not, then store it
            i = GetLastFrame();
            bool frameIsDiscardable = (Math.Abs(_frames[i][0] - snapshot[0]) < 10 && _frames[i][0] > snapshot[0]);
            if (frameIsDiscardable)
            {
                return false;
            }
            bool frameIsEqual = Utils.FramesAreEqual(snapshot, _frames[i]);
            if (frameIsEqual)
            {
                _frames[i][0] = snapshot[0];
                return true;
            }
            bool bufferIsFull = (i == _frames.Length - 1);
            if (bufferIsFull)
            {
                _frames[i - 1] = snapshot;
            }

            _frames[i+1] = snapshot;
            return true;
        }

        public byte CurrentSnapshotId()
        {
            return _frames[GetLastFrame()][0];
        }


        public void SetCharId(byte charId)
        {
            _charId = charId;
        }

        private int SnapshotIsPredicted(byte[] snapshot)
        {
            byte snapshotId = snapshot[0];
            for (int i = 0; i < _frames.Length; i++)
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            if (Frame.FramesAreEqual(_frames[1], frame))
            {
                return;
=======
            _frames[_frames.Length - 1] = null;
            return nextFrame;
        }
        
        private byte[] Interpolate(byte[] frame1, byte[] frame2)
        {
            byte[] interpolatedFrame = new byte[frame2.Length];
            interpolatedFrame[0] = frame2[0];
            for (int j = 1; j < frame2.Length;)
            {
                interpolatedFrame[j] = frame2[j];
                j++;
                interpolatedFrame[j] = frame2[j];
                j++;
                Vector3 lastFramePos    = j > frame1.Length ? Vector3.zero : Utils.ByteArrayToVector3(frame1, j);
                Vector3 newFramePos     = Utils.ByteArrayToVector3(frame2, j);
                Vector3 interpolatedPos = (lastFramePos + newFramePos) / 2;
                Utils.Vector3ToByteArray(interpolatedPos, interpolatedFrame, j);
                j += UnreliableStream.PACKET_SIZE-2;
>>>>>>> Stashed changes
            }
            _frames[2] = frame;
        }
       
    }
}