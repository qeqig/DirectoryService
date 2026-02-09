import { Card } from "@/shared/components/ui/card";
import { Location } from "@/entities/locations/type";

type Props = {
    location: Location;
};

export default function LocationCard({ location }: Props) {
    return (
        <Card
            key={location.id}
            className="bg-slate-900/40 border-slate-700 px-4"
        >
            <div className="flex justify-between items-start">
                <div>
                    <h3 className="text-lg font-medium text-white">
                        {location.name}
                    </h3>
                    <div className="text-sm text-slate-400 mt-1">
                        {location.city}, {location.street},{" "}
                        {location.houseNumber}
                    </div>
                </div>
                <div>
                    {
                        <span
                            className={
                                "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium " +
                                (location.isActive
                                    ? "bg-emerald-100 text-emerald-800"
                                    : "bg-amber-100 text-amber-800")
                            }
                        >
                            {location.isActive ? "Active" : "Deactived"}
                        </span>
                    }
                </div>
            </div>

            <div className="mt-4 text-sm text-slate-300">
                Часовой пояс: {location.timezone}
            </div>
        </Card>
    );
}
