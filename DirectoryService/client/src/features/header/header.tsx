import Image from "next/image";
import {
    Avatar,
    AvatarFallback,
    AvatarImage,
} from "@/shared/components/ui/avatar";
import Link from "next/link";
import { routes } from "@/shared/routes";
import { SidebarTrigger } from "@/shared/components/ui/sidebar";
import { Separator } from "@/shared/components/ui/separator";

export default function Header() {
    return (
        <header className="sticky top-0 z-10 flex h-14 items-center gap-2 border-b bg-background px-4">
            <SidebarTrigger />

            <Separator orientation="vertical" className="mr-2 h-4" />

            <Link href={routes.home} className="flex items-center gap-2">
                <Image
                    src="/globe.svg"
                    alt="Logo"
                    width={30}
                    height={30}
                    className="h-5 w-5"
                />
                <span className="text-lg font-bold hidden sm:inline-block">
                    Directory Service
                </span>
            </Link>

            <Avatar className="h-8 w-8 ml-auto">
                <AvatarImage src="https://github.com/shadcn.png" alt="@user" />
                <AvatarFallback>US</AvatarFallback>
            </Avatar>
        </header>
    );
}
