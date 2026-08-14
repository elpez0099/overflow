'use client';

import { Button } from "@heroui/react";
import { useTheme } from "next-themes";
import { MoonIcon, SunIcon } from "@heroicons/react/16/solid";

export default function ThemeToggle() {
    const { theme, setTheme } = useTheme();

    return (
        <Button
            isIconOnly
            aria-label="Toggle theme"
            onPress={() => {
                const isDark = theme === "dark";

                setTheme(isDark ? "light" : "dark");
            }}
        >
            <MoonIcon className="size-5 dark:hidden" />
            <SunIcon className="hidden size-5 dark:block" />
            
        </Button>
    );
}