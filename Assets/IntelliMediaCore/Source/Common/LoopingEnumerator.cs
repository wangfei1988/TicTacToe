﻿//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class LoopingEnumerator<T> : IEnumerator<T> 
	{
        List<T> _list;
        Func<T, bool> _filter;
        T _lastItem;
        bool _looped;
        int _index;

        public LoopingEnumerator(List<T> list, Func<T, bool> filter)
        {
            _list = list;
            _filter = filter;
        }

        bool NextIndex()
        {
            if (_list.Count > 0)
            {
                _index--;
                if (_index < 0)
                {
                    _index = _list.Count - 1;
                }

                return true;
            }
            else
            {
                _index = -1;
                return false;
            }
        }

        #region IEnumerator implementation
        public bool MoveNext()
        {
            while (!_looped && NextIndex())
            {
                if (_lastItem == null || _lastItem.Equals(Current))
                {
                    _looped = true;
                }

                if (_filter == null || _filter(Current))
                {
                    return true;
                }
            } 

            return false;
        }

        public void Reset()
        {
            _lastItem = Current;
            _looped = false;
        }

        object IEnumerator.Current 
        {
            get 
            {
                return Current;
            }
        }
        #endregion

        #region IDisposable implementation
        public void Dispose ()
        {
        }
        #endregion

        #region IEnumerator implementation
        public T Current 
        {
            get 
            {
                if (_index < _list.Count)
                {
                    return _list[_index];
                }
                else
                {
                    return default(T);
                }
            }
        }
        #endregion
    }
}
