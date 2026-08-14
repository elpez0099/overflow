'use client'
import {
    HomeIcon,
    QuestionMarkCircleIcon,
    TagIcon,
    UserIcon
} from "@heroicons/react/24/solid";

import { ListBox } from "@heroui/react";
import {usePathname, useRouter} from "next/navigation";

export default function SideMenu() {
    // Esta linea nos permite obtener el path de la url actual, por ejemplo /tags
    const pathname = usePathname();
    const navLinks = [
        {
            id: "home",
            icon: HomeIcon,
            text: "Home",
            href: "/"
        },
        {
            id: "questions",
            icon: QuestionMarkCircleIcon,
            text: "Questions",
            href: "/questions"
        },
        {
            id: "tags",
            icon: TagIcon,
            text: "Tags",
            href: "/tags"
        },
        {
            id: "session",
            icon: UserIcon,
            text: "User Session",
            href: "/session"
        }
    ];

    return (
        <ListBox
            aria-label="Navigation"
            className="ml-0 bg-default-100/50 p-2 pl-0 backdrop-blur-md rounded-medium"
        >
            {navLinks.map(({ id, href, icon: Icon, text }) => (
                <ListBox.Item
                    key={id}
                    id={id}
                    href={href}
                    textValue={text}
                    className={
                        pathname === href
                            ? "text-accent" // Si el path actual coincide con el href del elemento, el texto se muestra en un color que resalta
                            : "text-muted" // por el contrario, si no coincide se muestra en color mas tenue
                    }
                >
                    <Icon className="h-6 w-6" />
                    {text}
                </ListBox.Item>
            ))}
        </ListBox>
    );
}