import Link from "next/link";
import "./globals.css";

export default function DirectoryLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <html lang="en">
            <body className="flex min-h-screen bg-gray-50">
                <aside className="w-60 bg-gray-400 border-r p-5">
                    <h1 className="text-xl font-semibold mb-5">
                        Directory Service
                    </h1>

                    <nav className="flex flex-col gap-2 text-red-700">
                        <Link href="/" className="p-2 rounded hover:bg-red-300">
                            Главная
                        </Link>

                        <Link
                            href="/positions"
                            className="p-2 rounded hover:bg-red-300"
                        >
                            Positions
                        </Link>
                        <Link
                            href="/locations"
                            className="p-2 rounded hover:bg-red-300"
                        >
                            Locations
                        </Link>
                        <Link
                            href="/departments"
                            className="p-2 rounded hover:bg-red-300"
                        >
                            Departments
                        </Link>
                    </nav>
                </aside>

                <div className="flex-1 flex flex-col">
                    <header className="bg-white border-b p-4">
                        <h1 className="text-2xl font-medium">
                            Directory Service
                        </h1>
                    </header>

                    <main className="p-6">{children}</main>
                </div>
            </body>
        </html>
    );
}

