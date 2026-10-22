using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo.SemanticSearch {
    public class CachingEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>> {
        readonly IEmbeddingGenerator<string, Embedding<float>> generator;
        readonly Dictionary<string, Embedding<float>> cache;

        public CachingEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> generator) {
            this.generator = generator;
            cache = LoadCache() ?? new Dictionary<string, Embedding<float>>();
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
            EmbeddingGenerationOptions options = null, CancellationToken cancellationToken = default(CancellationToken)) {
            var newValues = values.Except(cache.Keys).ToList();
            UsageDetails usageDetails = null;
            if(newValues.Count > 0) {
                var newEmbeddings = await generator.GenerateAsync(newValues, options, cancellationToken).ConfigureAwait(false);
                usageDetails = newEmbeddings.Usage;
                for(var i = 0; i < newValues.Count; i++)
                    cache[newValues[i]] = newEmbeddings[i];
            }

            var embeddings = new List<Embedding<float>>();
            foreach(var value in values)
                embeddings.Add(cache.TryGetValue(value, out var embedding) ? embedding : null);

            return new GeneratedEmbeddings<Embedding<float>>(embeddings) { Usage = usageDetails };
        }

        public object GetService(Type serviceType, object serviceKey = null) {
            return generator.GetService(serviceType, serviceKey);
        }

        public void Dispose() {
            generator.Dispose();
        }

        static Dictionary<string, Embedding<float>> LoadCache() {
            var assembly = typeof(CachingEmbeddingGenerator).Assembly;
            var stream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "MovieVectors.json");
            if(stream == null)
                return null;
            return JsonSerializer.Deserialize<Dictionary<string, Embedding<float>>>(stream);
        }
    }
}
