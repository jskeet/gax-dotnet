/*
 * Copyright 2019 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Api.Gax.Grpc.Testing
{
    // TODO: Consider accepting an IAsyncEnumerable<T> instead, then creating the enumerator by passing in a cancellation
    // token that we can cancel in any MoveNext call.

#if NET45
    /// <summary>
    /// Simple adapter to allow an <see cref="ICompatibilityAsyncEnumerator{T}"/> to be used as a gRPC <see cref="IAsyncStreamReader{T}"/>.
    /// Note that cancellation is not fully supported, due to differences between the two interfaces.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public sealed class AsyncStreamAdapter<T> : IAsyncStreamReader<T>
#else
    /// <summary>
    /// Simple adapter to allow an <see cref="IAsyncEnumerator{T}"/> to be used as a gRPC <see cref="IAsyncStreamReader{T}"/>.
    /// Note that cancellation is not fully supported, due to differences between the two interfaces.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public sealed class AsyncStreamAdapter<T> : IAsyncStreamReader<T>
#endif
    {
        private readonly ICompatibilityAsyncEnumerator<T> _enumerator;

#if !NET45
        /// <summary>
        /// Wraps the given async enumerator as an async stream reader.
        /// </summary>
        /// <param name="enumerator">The enumerator to wrap</param>
        public AsyncStreamAdapter(IAsyncEnumerator<T> enumerator) =>
            _enumerator = new EnumeratorWrapper(enumerator);

        private class EnumeratorWrapper : ICompatibilityAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> _enumerator;
            internal EnumeratorWrapper(IAsyncEnumerator<T> enumerator) =>
                _enumerator = enumerator;

            /// <inheritdoc />
            public T Current => _enumerator.Current;

            public ValueTask DisposeAsync() => _enumerator.DisposeAsync();

            /// <inheritdoc />
            public ValueTask<bool> MoveNextAsync() => _enumerator.MoveNextAsync();
        }
#endif

        /// <summary>
        /// Wraps the given async enumerator as an async stream reader.
        /// </summary>
        /// <param name="enumerator">The enumerator to wrap</param>
        public AsyncStreamAdapter(ICompatibilityAsyncEnumerator<T> enumerator) =>
            _enumerator = enumerator;

        /// <inheritdoc />
        public T Current => _enumerator.Current;

        /// <inheritdoc />
        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _enumerator.MoveNextAsync().ConfigureAwait(false);
        }
    }
}
