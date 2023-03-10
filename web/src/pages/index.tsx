import Link from "@docusaurus/Link";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import clsx from "clsx";
import React from "react";

import styles from "./index.module.css";

function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <header
      className={clsx(
        "hero hero--primary bg-gradient-to-br from-pink-500 via-red-500 to-yellow-500",
        styles.heroBanner
      )}
    >
      <div className="container">
        <h1 className="hero__title">{siteConfig.title}</h1>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={clsx(styles.buttons)}>
          <Link
            className="button button--secondary button--lg shadow-xl"
            to="/docs/intro"
          >
            👉 Easy &amp; quick installation
          </Link>
        </div>
      </div>
    </header>
  );
}

export default function Home(): JSX.Element {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`VBC Wireless Gamepad`}
      description="Use your VBar Control for any Simulator or game"
    >
      <HomepageHeader />
      <main>
        <section className="bg-blue py-20">
          <div className="flex items-center justify-center gap-8">
            <div className="flex flex-col">
              <img src="/img/vbar1.png" className="w-64" />
            </div>
            <div className="text-6xl">+</div>
            <div className="flex flex-col">
              <div className="text-semibold text-center text-xl">
                Any Simulator
              </div>
              <div className="text-center italic">or</div>
              <div className="text-center">Game</div>
            </div>
            <div className="text-6xl">=</div>
            <div className="text-6xl">❤️</div>
          </div>
        </section>
        <section>
          <div className="flex flex-col items-center gap-8">
            <h2 className="text-semibold text-3xl">Filling the Gap</h2>
            <p className="max-w-4xl text-center">
              The VBC Wireless Gamepad application enables wireless control of a
              generic Windows game controller using a VBarControl Touch,
              allowing it to be used with a wide range of flight simulators and
              games. While some simulators natively support the protocol
              developed by Mikado, they remain in the minority. The VBC Wireless
              Gamepad fills this gap by providing a versatile solution that can
              be used with any simulator or game that supports Windows
              joysticks.
            </p>
          </div>
        </section>
        <section className="mb-20  py-14">
          <div className="flex flex-col items-center">
            <img
              src="/img/demo.gif"
              alt="demo"
              className="max-w-3xl shadow-lg"
            />
            <p className="mt-1 text-sm italic text-gray-600">
              VBCWirelessGamepad in action
            </p>
          </div>
        </section>
      </main>
    </Layout>
  );
}
