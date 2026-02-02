"use client";

import { useMemo, useState } from "react";
import { buildApiUrl } from "@/lib/api";

const minProblemLength = 20;
const maxProblemLength = 2000;

type PromptDetails = {
  analysis?: string;
  context?: string;
};

type GeneratePromptResponse = {
  success: boolean;
  optimizedPrompt?: string;
  details?: PromptDetails;
  error?: string;
};

export default function PromptForm() {
  const [problem, setProblem] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<GeneratePromptResponse | null>(null);
  const [copied, setCopied] = useState(false);

  const remaining = maxProblemLength - problem.length;
  const isTooShort = problem.trim().length > 0 && problem.trim().length < minProblemLength;

  const canSubmit = useMemo(() => {
    const length = problem.trim().length;
    return length >= minProblemLength && length <= maxProblemLength && !isLoading;
  }, [problem, isLoading]);

  const handleCopy = async () => {
    if (!result?.optimizedPrompt) {
      return;
    }

    try {
      await navigator.clipboard.writeText(result.optimizedPrompt);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      setCopied(false);
    }
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setResult(null);

    const trimmed = problem.trim();
    if (!trimmed) {
      setError("Please describe the problem you want to solve.");
      return;
    }

    if (trimmed.length < minProblemLength) {
      setError(`Please provide at least ${minProblemLength} characters.`);
      return;
    }

    if (trimmed.length > maxProblemLength) {
      setError(`Please keep your request under ${maxProblemLength} characters.`);
      return;
    }

    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 30000);

    try {
      setIsLoading(true);
      const response = await fetch(buildApiUrl("/api/generate-prompt"), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ problem: trimmed }),
        signal: controller.signal,
      });

      const payload = (await response.json()) as GeneratePromptResponse;

      if (!response.ok || !payload.success) {
        setError(payload.error ?? "We couldn't generate a prompt just now. Please try again.");
        return;
      }

      setResult(payload);
    } catch (err) {
      if ((err as Error).name === "AbortError") {
        setError("The request timed out. Please try again.");
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      clearTimeout(timeout);
      setIsLoading(false);
    }
  };

  return (
    <div className="rounded-3xl bg-white/70 p-8 shadow-xl shadow-brand-900/10 backdrop-blur sm:p-10">
      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label className="text-sm font-semibold uppercase tracking-wide text-brand-600">
            Describe your problem
          </label>
          <textarea
            value={problem}
            onChange={(event) => setProblem(event.target.value)}
            rows={6}
            maxLength={maxProblemLength}
            className="mt-3 w-full rounded-2xl border border-brand-200/50 bg-white px-4 py-3 text-base text-brand-900 shadow-sm outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-200"
            placeholder="Example: We need a secure, scalable API for managing customer onboarding with role-based access and audit logging."
          />
          <div className="mt-2 flex items-center justify-between text-sm text-brand-600">
            <span>{isTooShort ? `Add ${minProblemLength - problem.trim().length} more characters.` : "Be as specific as possible."}</span>
            <span>{remaining} characters left</span>
          </div>
        </div>

        {error ? (
          <div className="rounded-xl border border-brand-400/40 bg-brand-50/70 px-4 py-3 text-sm text-brand-600">
            {error}
          </div>
        ) : null}

        <button
          type="submit"
          disabled={!canSubmit}
          className="flex w-full items-center justify-center gap-2 rounded-full bg-brand-400 px-6 py-3 text-base font-semibold text-white shadow-lg shadow-brand-400/30 transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:bg-brand-200"
        >
          {isLoading ? "Generating prompt..." : "Generate prompt"}
        </button>
      </form>

      {result?.optimizedPrompt ? (
        <div className="mt-8 space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-brand-900">Your optimized prompt</h3>
            <button
              type="button"
              onClick={handleCopy}
              className="rounded-full border border-brand-200 px-4 py-2 text-sm font-semibold text-brand-600 transition hover:border-brand-400"
            >
              {copied ? "Copied" : "Copy"}
            </button>
          </div>
          <div className="rounded-2xl border border-brand-200/60 bg-white px-5 py-4 text-sm leading-7 text-brand-900">
            {result.optimizedPrompt}
          </div>

          {(result.details?.analysis || result.details?.context) && (
            <details className="rounded-2xl border border-brand-200/50 bg-white/80 px-5 py-4 text-sm text-brand-600">
              <summary className="cursor-pointer font-semibold text-brand-600">
                See how we refined your prompt
              </summary>
              {result.details?.analysis ? (
                <div className="mt-3">
                  <p className="text-xs font-semibold uppercase tracking-wide text-brand-400">
                    Analysis
                  </p>
                  <p className="mt-2 text-sm leading-6 text-brand-900">
                    {result.details.analysis}
                  </p>
                </div>
              ) : null}
              {result.details?.context ? (
                <div className="mt-4">
                  <p className="text-xs font-semibold uppercase tracking-wide text-brand-400">
                    Context enrichment
                  </p>
                  <p className="mt-2 text-sm leading-6 text-brand-900">
                    {result.details.context}
                  </p>
                </div>
              ) : null}
            </details>
          )}
        </div>
      ) : null}
    </div>
  );
}
