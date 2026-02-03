import GeneratorTabs from "@/components/GeneratorTabs";

const features = [
  {
    title: "Multi-stage prompt optimization",
    description:
      "We analyze, enrich, and refine your problem statement to deliver a production-ready prompt.",
  },
  {
    title: "Designed for real-world teams",
    description:
      "Capture domain constraints, success criteria, and context so your AI outputs stay on target.",
  },
  {
    title: "Fast feedback loop",
    description:
      "Get a polished prompt in seconds, with optional analysis you can share across your org.",
  },
];

const steps = [
  {
    title: "Describe the problem",
    description:
      "Tell us what you need to solve, including goals, constraints, and the target audience.",
  },
  {
    title: "We enrich your context",
    description:
      "Our pipeline extracts missing details and injects best-practice guidance.",
  },
  {
    title: "Get a ready-to-run prompt",
    description:
      "Use the optimized prompt with your AI tools immediately or share it with your team.",
  },
];

const testimonials = [
  {
    quote:
      "Aiomi helped our onboarding team standardize AI prompts and cut editing time in half.",
    name: "Maya Patel",
    role: "Head of Customer Success",
  },
  {
    quote:
      "We turned vague requests into actionable prompts that our engineers can ship with confidence.",
    name: "Jordan Lee",
    role: "VP of Product",
  },
  {
    quote:
      "The clarity we get from the analysis step is a game-changer for cross-team alignment.",
    name: "Rafael Gomez",
    role: "Tech Lead",
  },
];

const faqs = [
  {
    question: "What kind of problems work best?",
    answer:
      "Anything that needs clarity: product requirements, technical designs, research briefs, or customer workflows.",
  },
  {
    question: "Can we add authentication later?",
    answer:
      "Yes. The architecture is ready for role-based access and enterprise-grade identity integration.",
  },
  {
    question: "Is the prompt editable?",
    answer:
      "Absolutely. Copy the output, iterate, and collaborate with your team on the final prompt.",
  },
];

export default function Home() {
  return (
    <div className="min-h-screen bg-brand-50 text-brand-900">
      <header className="mx-auto flex max-w-6xl items-center justify-between px-6 py-6">
        <div className="text-lg font-semibold tracking-wide text-brand-900">
          Aiomi
        </div>
        <nav className="hidden items-center gap-6 text-sm font-semibold text-brand-600 md:flex">
          <a href="#features" className="transition hover:text-brand-900">
            Features
          </a>
          <a href="#workflow" className="transition hover:text-brand-900">
            How it works
          </a>
          <a href="#pricing" className="transition hover:text-brand-900">
            Pricing
          </a>
          <a href="#faq" className="transition hover:text-brand-900">
            FAQ
          </a>
        </nav>
        <a
          href="#prompt"
          className="rounded-full bg-brand-400 px-5 py-2 text-sm font-semibold text-white shadow-md shadow-brand-400/30 transition hover:bg-brand-600"
        >
          Generate a prompt
        </a>
      </header>

      <main className="mx-auto flex max-w-6xl flex-col gap-20 px-6 pb-20">
        <section className="grid items-center gap-12 pt-10 md:grid-cols-[1.1fr_0.9fr]">
          <div className="space-y-6">
            <span className="inline-flex items-center rounded-full bg-white/70 px-4 py-2 text-xs font-semibold uppercase tracking-[0.2em] text-brand-600">
              AI prompt studio for teams
            </span>
            <h1 className="text-4xl font-semibold leading-tight text-brand-900 md:text-5xl">
              Turn business problems into production-ready AI prompts.
            </h1>
            <p className="text-lg leading-8 text-brand-600">
              Aiomi helps consultancies and SaaS teams capture the right context, align stakeholders, and ship AI-ready prompts in minutes.
            </p>
            <div className="flex flex-col gap-4 sm:flex-row">
              <a
                href="#prompt"
                className="rounded-full bg-brand-400 px-6 py-3 text-center text-sm font-semibold text-white shadow-lg shadow-brand-400/30 transition hover:bg-brand-600"
              >
                Try it now
              </a>
              <a
                href="#features"
                className="rounded-full border border-brand-200 bg-white px-6 py-3 text-center text-sm font-semibold text-brand-600 transition hover:border-brand-400"
              >
                View features
              </a>
            </div>
          </div>
          <div className="rounded-3xl bg-brand-900 px-8 py-10 text-brand-50 shadow-2xl shadow-brand-900/40">
            <p className="text-sm uppercase tracking-widest text-brand-200">
              Sample output
            </p>
            <p className="mt-4 text-lg leading-8">
              &quot;You are an enterprise onboarding strategist. Create a step-by-step workflow that reduces time-to-value for new customers while ensuring compliance, audit logging, and role-based access. Provide the workflow as a checklist with owners, dependencies, and success metrics.&quot;
            </p>
            <p className="mt-6 text-sm text-brand-200">
              Optimized by Aiomi
            </p>
          </div>
        </section>

        <section id="prompt" className="grid gap-10 lg:grid-cols-[1fr_1.1fr]">
          <div className="space-y-6">
            <h2 className="text-3xl font-semibold text-brand-900">
              Generate your optimized prompt
            </h2>
            <p className="text-base leading-7 text-brand-600">
              Describe the challenge, product, or workflow you want to solve. We will return a refined prompt that your team can use immediately.
            </p>
            <div className="rounded-2xl border border-brand-200/60 bg-white/70 px-6 py-5 text-sm text-brand-600">
              <p className="font-semibold text-brand-900">Best results include:</p>
              <ul className="mt-3 list-disc space-y-2 pl-5">
                <li>Who the prompt is for and expected outcomes</li>
                <li>Constraints, compliance rules, or tools to use</li>
                <li>Preferred format (bullets, tables, steps)</li>
              </ul>
            </div>
          </div>
          <GeneratorTabs />
        </section>

        <section id="features" className="space-y-10">
          <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
            <div>
              <h2 className="text-3xl font-semibold text-brand-900">Built for modern AI workflows</h2>
              <p className="mt-3 text-base leading-7 text-brand-600">
                Everything you need to standardize AI prompt quality and drive consistent outcomes.
              </p>
            </div>
            <a
              href="#prompt"
              className="text-sm font-semibold text-brand-600 transition hover:text-brand-900"
            >
              Start generating →
            </a>
          </div>
          <div className="grid gap-6 md:grid-cols-3">
            {features.map((feature) => (
              <div
                key={feature.title}
                className="rounded-3xl border border-brand-200/40 bg-white/70 p-6 shadow-lg shadow-brand-900/5"
              >
                <h3 className="text-lg font-semibold text-brand-900">{feature.title}</h3>
                <p className="mt-3 text-sm leading-6 text-brand-600">{feature.description}</p>
              </div>
            ))}
          </div>
        </section>

        <section id="workflow" className="rounded-3xl bg-brand-900 px-8 py-12 text-brand-50">
          <div className="grid gap-10 md:grid-cols-[1fr_1.2fr]">
            <div>
              <h2 className="text-3xl font-semibold">How it works</h2>
              <p className="mt-4 text-base leading-7 text-brand-200">
                Our pipeline mirrors how elite consultants gather requirements: diagnose, enrich, and translate into clear instructions.
              </p>
            </div>
            <div className="space-y-6">
              {steps.map((step, index) => (
                <div key={step.title} className="flex gap-4">
                  <span className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-400 text-sm font-semibold text-white">
                    {index + 1}
                  </span>
                  <div>
                    <h3 className="text-lg font-semibold text-brand-50">{step.title}</h3>
                    <p className="mt-2 text-sm leading-6 text-brand-200">{step.description}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="grid gap-6 md:grid-cols-3">
          {testimonials.map((testimonial) => (
            <div
              key={testimonial.name}
              className="rounded-3xl border border-brand-200/40 bg-white/80 p-6 text-sm text-brand-600"
            >
              <p className="text-base leading-7 text-brand-900">“{testimonial.quote}”</p>
              <p className="mt-4 font-semibold text-brand-900">{testimonial.name}</p>
              <p className="text-xs uppercase tracking-wide text-brand-400">{testimonial.role}</p>
            </div>
          ))}
        </section>

        <section id="pricing" className="rounded-3xl border border-brand-200/50 bg-white/80 p-10">
          <div className="grid gap-10 md:grid-cols-[1fr_1fr]">
            <div>
              <h2 className="text-3xl font-semibold text-brand-900">Pricing that scales with your team</h2>
              <p className="mt-4 text-base leading-7 text-brand-600">
                Start with a lightweight plan today and upgrade when you need enterprise controls, workflows, and analytics.
              </p>
              <ul className="mt-6 space-y-3 text-sm text-brand-600">
                <li>• Starter: prompt generation for small teams</li>
                <li>• Growth: shared prompt libraries and approvals</li>
                <li>• Enterprise: SSO, audit trails, and SLAs</li>
              </ul>
            </div>
            <div className="rounded-3xl bg-brand-900 px-8 py-10 text-brand-50">
              <p className="text-sm uppercase tracking-widest text-brand-200">Launch plan</p>
              <p className="mt-4 text-4xl font-semibold">$0</p>
              <p className="mt-2 text-sm text-brand-200">Pilot Aiomi with your team.</p>
              <a
                href="#prompt"
                className="mt-6 inline-flex rounded-full bg-brand-400 px-5 py-3 text-sm font-semibold text-white transition hover:bg-brand-600"
              >
                Start a pilot
              </a>
            </div>
          </div>
        </section>

        <section id="faq" className="space-y-8">
          <h2 className="text-3xl font-semibold text-brand-900">Frequently asked questions</h2>
          <div className="grid gap-6 md:grid-cols-2">
            {faqs.map((faq) => (
              <div
                key={faq.question}
                className="rounded-3xl border border-brand-200/40 bg-white/80 p-6"
              >
                <p className="text-base font-semibold text-brand-900">{faq.question}</p>
                <p className="mt-3 text-sm leading-6 text-brand-600">{faq.answer}</p>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-3xl bg-brand-400 px-8 py-10 text-white">
          <div className="flex flex-col items-start gap-6 md:flex-row md:items-center md:justify-between">
            <div>
              <h2 className="text-3xl font-semibold">Ready to optimize your next prompt?</h2>
              <p className="mt-2 text-base text-white/90">
                Bring clarity to every AI request and align your team faster.
              </p>
            </div>
            <a
              href="#prompt"
              className="rounded-full bg-white px-6 py-3 text-sm font-semibold text-brand-900"
            >
              Generate a prompt
            </a>
          </div>
        </section>
      </main>

      <footer className="border-t border-brand-200/40 bg-brand-50/80">
        <div className="mx-auto flex max-w-6xl flex-col gap-4 px-6 py-8 text-sm text-brand-600 md:flex-row md:items-center md:justify-between">
          <p>© 2026 Aiomi. All rights reserved.</p>
          <div className="flex gap-6">
            <a href="#features" className="transition hover:text-brand-900">
              Features
            </a>
            <a href="#pricing" className="transition hover:text-brand-900">
              Pricing
            </a>
            <a href="#faq" className="transition hover:text-brand-900">
              FAQ
            </a>
          </div>
        </div>
      </footer>
    </div>
  );
}
